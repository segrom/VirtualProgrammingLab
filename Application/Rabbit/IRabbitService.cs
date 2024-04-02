namespace Application.Rabbit;

public interface IRabbitService
{
    void SendMessage(object obj);
    void SendMessage(string message);
}