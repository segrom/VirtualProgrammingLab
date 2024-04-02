using System.Diagnostics;
using System.Text;
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
        // Не забудьте вынести значения "localhost" и "MyQueue"
        // в файл конфигурации
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMqSend:Hostname"],
            UserName = _configuration["RabbitMqSend:UserName"],
            Password = _configuration["RabbitMqSend:Password"]
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _configuration["RabbitMqSend:QueueName"], durable: false, 
            exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());

            _logger.Log(LogLevel.Information,$"Code received: {content}");
            RunUser.Instance.RunCode(content);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(_configuration["RabbitMqSend:QueueName"], false, consumer);

        return Task.CompletedTask;
    }
	
    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}