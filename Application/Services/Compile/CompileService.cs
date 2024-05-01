using System.Text;
using System.Text.Json;
using Common;
using Common.Common;
using Common.QueueStructures;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Application.Services.Compile;

public class CompileService: ICompileService
{
    private event Action<QueueCompileResult> CompileResultReceived;
    
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<CompileService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Guid _serviceQueueId;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private readonly string _resultsQueueName;

    public CompileService(ILogger<CompileService> logger, IConfiguration configuration, 
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
        _serviceQueueId = Guid.NewGuid();
        _resultsQueueName = Environment.GetEnvironmentVariable("RESULTS_QUEUE_NAME_FORMAT").FormatWith(_serviceQueueId);
        _logger.LogInformation("Login rabbit as {0}", Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME"));
        var factory = new ConnectionFactory()
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
            UserName = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME"),
            Password = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD"),
            Port = 5672
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: _resultsQueueName, durable: false, 
            exclusive: false, autoDelete: true, arguments: null);
        _logger.Log(LogLevel.Information,$"Created CompileService with queue \'{_resultsQueueName}\'");

        StartAsync();
    }

    public async Task<QueueCompileResult> QueueCompileRequest(CompileRequest compileRequest)
    {
        var request = new QueueCompileRequest(
            _serviceQueueId, 
            compileRequest.Id, 
            compileRequest.Code,
            compileRequest.Tests,
            compileRequest.IsExercise);
        var body = JsonSerializer.SerializeToUtf8Bytes(request);

        var destinationQueue = Environment.GetEnvironmentVariable("INPUT_QUEUE_NAME_FORMAT")
            .FormatWith(compileRequest.Language.HighlightLabel);
        
        _channel.BasicPublish(exchange: "",
            routingKey: destinationQueue,
            basicProperties: null,
            body: body);
        _logger.Log(LogLevel.Information,$"Send source code (to queue {destinationQueue}) {body}");

        QueueCompileResult? result = null;

        CompileResultReceived += r =>
        {
            if (r.CompileRequestId != compileRequest.Id) return;
            result = r;
        };

        while (result is null)
        {
            await Task.Yield();
        }
        
        return result;
    }

    private void StartAsync()
    {
        _logger.LogInformation($"Start listening {_resultsQueueName} with compile service");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            var result = JsonSerializer.Deserialize<QueueCompileResult>(ea.Body.ToArray());

            if (result is null)
            {
                _logger.Log(LogLevel.Warning,$"Unable deserialize message: {Encoding.UTF8.GetString(ea.Body.ToArray())}");
            }
            else
            {
                CompileResultReceived(result);
                _logger.Log(LogLevel.Information,$"Receive compile results: {Encoding.UTF8.GetString(ea.Body.ToArray())}");
            }

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(_resultsQueueName, false, consumer);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _channel.Dispose();
    }
}
