using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Common.Structures;
using Humanizer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CompilerService;

public class RabbitListener : BackgroundService
{
    private readonly ILogger<RabbitListener> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitListener(ILogger<RabbitListener> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _logger.LogInformation("Login rabbit as {0}", Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME"));
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMqSend:Hostname"],
            UserName = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME"),
            Password = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD")
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _configuration["RabbitMqSend:CompileQueueName"], durable: false, 
            exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (ch, ea) =>
        {
            var request = JsonSerializer.Deserialize<QueueCompileRequest>(ea.Body.ToArray());

            if (request is null)
            {
                _logger.Log(LogLevel.Warning,$"Unable deserialize message: {Encoding.UTF8.GetString(ea.Body.ToArray())}");
                _channel.BasicAck(ea.DeliveryTag, false);
                return;
            }
            
            _logger.Log(LogLevel.Information,$"Receive request to compile {request.ServiceId}:{request.CompileRequestId}");
            var result = await CodeRunner.Instance.RunCode(request);
            var data = JsonSerializer.SerializeToUtf8Bytes(result);
            _channel.BasicPublish(exchange: "",
                routingKey: _configuration["RabbitMqSend:ResultsQueueNameFormat"].FormatWith(result.ServiceId),
                basicProperties: null,
                body: data);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(_configuration["RabbitMqSend:CompileQueueName"], false, consumer);

        return Task.CompletedTask;
    }
	
    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}