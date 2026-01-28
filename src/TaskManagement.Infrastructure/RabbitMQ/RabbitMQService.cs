using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TaskManagement.Infrastructure.RabbitMQ;

public interface IRabbitMQService
{
    void PublishMessage(string queueName, string message);
    void StartConsuming(string queueName, Action<string> onMessageReceived);
    void Dispose();
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly string _hostName;
    private bool _isConnected = false;

    public RabbitMQService(ILogger<RabbitMQService> logger, string hostName = "localhost")
    {
        _logger = logger;
        _hostName = hostName;
        TryConnect();
    }

    private void TryConnect()
    {
        try
        {
            var factory = new ConnectionFactory { HostName = _hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _isConnected = true;
            _logger.LogInformation("Successfully connected to RabbitMQ at {HostName}", _hostName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to RabbitMQ at {HostName}. Service will continue but reminders won't be processed. Start RabbitMQ with: docker compose -f docker/docker-compose.yml up -d rabbitmq", _hostName);
            _isConnected = false;
        }
    }

    public void PublishMessage(string queueName, string message)
    {
        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("Cannot publish message: RabbitMQ is not connected. Attempting to reconnect...");
            TryConnect();
            if (!_isConnected || _channel == null)
            {
                _logger.LogError("Failed to publish message to queue {QueueName}: RabbitMQ is not available", queueName);
                return;
            }
        }

        try
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            _logger.LogInformation("Published message to queue {QueueName}: {Message}", queueName, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to queue {QueueName}", queueName);
            _isConnected = false;
        }
    }

    public void StartConsuming(string queueName, Action<string> onMessageReceived)
    {
        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("Cannot start consuming: RabbitMQ is not connected. Attempting to reconnect...");
            TryConnect();
            if (!_isConnected || _channel == null)
            {
                _logger.LogError("Failed to start consuming from queue {QueueName}: RabbitMQ is not available", queueName);
                return;
            }
        }

        try
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message from queue {QueueName}: {Message}", queueName, message);
                onMessageReceived(message);
                _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming from queue: {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting consumer for queue {QueueName}", queueName);
            _isConnected = false;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing RabbitMQ connection");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
