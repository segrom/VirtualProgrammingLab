namespace VirtualProgrammingLab.RabbitMQ;

public interface IRabbitService: IServiceProvider
{
    void SendMessage(object obj);
    void SendMessage(string message);
}