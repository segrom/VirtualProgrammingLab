using System.Text;
using System.Text.Json;
using Common.Structures;
using Humanizer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Application.Rabbit;

public class CompileService: ICompileService
{
    private event Action<CompileResult> CompileResultReceived;
    
    private readonly ILogger<CompileService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Guid _serviceQueueId;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private readonly string _resultsQueueName;

    public CompileService(ILogger<CompileService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceQueueId = Guid.NewGuid();
        _resultsQueueName = _configuration["RabbitMqSend:ResultsQueueNameFormat"].FormatWith(_serviceQueueId);
        
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMqSend:Hostname"],
            UserName = _configuration["RabbitMqSend:UserName"],
            Password = _configuration["RabbitMqSend:Password"]
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: _resultsQueueName, durable: false, 
            exclusive: false, autoDelete: true, arguments: null);
        _logger.Log(LogLevel.Information,$"Created CompileService with queue \'{_resultsQueueName}\'");

        StartAsync();
    }

    public async Task<CompileResult> SendSourceCode(string sourceId, string code)
    {
        var request = new CompileRequest(
            _serviceQueueId,
            sourceId,
            code
            );
        var body = JsonSerializer.SerializeToUtf8Bytes(request);

        _channel.BasicPublish(exchange: "",
            routingKey: _configuration["RabbitMqSend:CompileQueueName"],
            basicProperties: null,
            body: body);
        _logger.Log(LogLevel.Information,$"Send source code (to queue {_configuration["RabbitMqSend:CompileQueueName"]}) {body}");

        CompileResult? result = null;

        CompileResultReceived += r =>
        {
            if (r.SourceId != sourceId) return;
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
            var result = JsonSerializer.Deserialize<CompileResult>(ea.Body.ToArray());

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
