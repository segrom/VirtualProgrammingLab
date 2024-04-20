using System.Text;
using System.Text.Json;
using Common;
using Common.Common;
using Common.Courses;
using Common.QueueStructures;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CompileResultsUpdaterService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly string _inputQueueName;
    
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public Worker(ILogger<Worker> logger, IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _logger.LogInformation("Login rabbit as {0}", Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME"));
        _inputQueueName = Environment.GetEnvironmentVariable("COMPILE_RESULTS_UPDATE_QUEUE_NAME") ?? throw new Exception("Environment variable not found");
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
            UserName = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME"),
            Password = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD"),
            Ssl = new SslOption()
            {
                ServerName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Enabled = false,
            }
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _inputQueueName,
            durable: false, 
            exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (ch, ea) =>
        {
            var result = JsonSerializer.Deserialize<QueueCompileResult>(ea.Body.ToArray());

            if (result is null)
            {
                _logger.Log(LogLevel.Warning,$"Unable deserialize message: {Encoding.UTF8.GetString(ea.Body.ToArray())}");
                _channel.BasicAck(ea.DeliveryTag, false);
                return;
            }

            await UpdateCompileResult(result);
            
            _logger.Log(LogLevel.Information,$"Receive results {result.ServiceId}:{result.CompileRequestId}");
            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(_inputQueueName, false, consumer);

        return Task.CompletedTask;
    }

    private async Task UpdateCompileResult(QueueCompileResult result)
    {
        var context = await _contextFactory.CreateDbContextAsync();
        var request = await context.CompileRequests
            .Include(r=>r.ExerciseState)
            .FirstAsync(l=>l.Id == result.CompileRequestId);
        
        request.Status = string.IsNullOrEmpty(result.ResultErrors)
            ? CompileRequestStatus.Finished
            : CompileRequestStatus.FinishedWithErrors;
        request.Output = result.ResultOutput;
        request.Errors = result.ResultErrors;
        request.FinishTime = result.FinishTime;
        request.Duration = result.Duration;
        
        if (request.ExerciseState != null && request.ExerciseState.Status != ExerciseStatus.Completed)
        {
            request.ExerciseState.Status = request.Status == CompileRequestStatus.Finished
                ? ExerciseStatus.Completed
                : ExerciseStatus.Failed;
        }
        
        context.Update(request);
        await context.SaveChangesAsync();
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}