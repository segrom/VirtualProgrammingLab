using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Application.Rabbit;

public class RabbitService: IRabbitService
{
    private readonly IConfiguration _configuration;

    public RabbitService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendMessage(object obj)
    {
        var message = JsonSerializer.Serialize(obj);
        SendMessage(message);
    }

    public void SendMessage(string message)
    {
        // Не забудьте вынести значения "localhost" и "MyQueue"
        // в файл конфигурации
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMqSend:Hostname"],
            UserName = _configuration["RabbitMqSend:UserName"],
            Password = _configuration["RabbitMqSend:Password"]
        };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: _configuration["RabbitMqSend:QueueName"],
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: _configuration["RabbitMqSend:QueueName"],
                basicProperties: null,
                body: body);
        }
    }
}