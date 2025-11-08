using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitMqPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqPublisher()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare("analytics.raw", ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare("analytics.raw.q", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("analytics.raw.q", "analytics.raw", "");
    }

    public void Publish(CombinedRecord record)
    {
        var json = JsonSerializer.Serialize(record);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "analytics.raw", routingKey: "", body: body);
    }
}
