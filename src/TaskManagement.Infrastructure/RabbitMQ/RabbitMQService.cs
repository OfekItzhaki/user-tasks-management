using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TaskManagement.Infrastructure.RabbitMQ;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly RabbitMQConnection _connection;
    private readonly ILogger<RabbitMQService> _logger;
    private string? _consumerTag;
    private volatile int _inFlightCount;
    private readonly object _inFlightLock = new();
    private const int ShutdownWaitSeconds = 30;

    public RabbitMQService(ILogger<RabbitMQService> logger, string hostName = "localhost")
    {
        _logger = logger;
        _connection = new RabbitMQConnection(logger, hostName);
    }

    public void PublishMessage(string queueName, string message, IReadOnlyDictionary<string, string>? headers = null)
    {
        if (!EnsureConnected())
        {
            if (IsLocalMode())
                _logger.LogInformation("RabbitMQ not available (Local mode). Skipping publish to {QueueName}.", queueName);
            else
                _logger.LogError("Failed to publish message to queue {QueueName}: RabbitMQ is not available", queueName);
            return;
        }

        try
        {
            _connection.Channel!.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);
            var props = _connection.Channel.CreateBasicProperties();
            props.Persistent = true;
            if (headers != null && headers.Count > 0)
            {
                props.Headers = new Dictionary<string, object?>();
                foreach (var (k, v) in headers)
                    props.Headers[k] = v;
            }
            _connection.Channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
            _logger.LogInformation("Published message to queue {QueueName}: {Message}", queueName, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to queue {QueueName}", queueName);
            _connection.MarkDisconnected();
        }
    }

    public void StartConsuming(string queueName, Func<string, bool> onMessageReceived)
    {
        if (!EnsureConnected())
        {
            if (IsLocalMode())
                _logger.LogInformation("RabbitMQ not available (Local mode). Reminder queue {QueueName} will not be consumed.", queueName);
            else
                _logger.LogError("Failed to start consuming from queue {QueueName}: RabbitMQ is not available", queueName);
            return;
        }

        try
        {
            _connection.Channel!.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(_connection.Channel);
            consumer.Received += (_, ea) =>
            {
                Interlocked.Increment(ref _inFlightCount);
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message from queue {QueueName}: {Message}", queueName, message);
                bool success = false;
                try
                {
                    success = onMessageReceived(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {QueueName}: {Message}", queueName, message);
                }
                finally
                {
                    Interlocked.Decrement(ref _inFlightCount);
                }
                try
                {
                    if (success)
                        _connection.Channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    else
                        _connection.Channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ack/nack for delivery {DeliveryTag}", ea.DeliveryTag);
                }
            };
            _consumerTag = _connection.Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming from queue: {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting consumer for queue {QueueName}", queueName);
            _connection.MarkDisconnected();
        }
    }

    public void StopConsuming()
    {
        try
        {
            if (string.IsNullOrEmpty(_consumerTag))
            {
                _consumerTag = null;
                return;
            }

            // Graceful shutdown: wait for in-flight messages to complete before cancelling
            var deadline = DateTime.UtcNow.AddSeconds(ShutdownWaitSeconds);
            while (_inFlightCount > 0 && DateTime.UtcNow < deadline)
            {
                _logger.LogDebug("Waiting for {Count} in-flight message(s) to complete before shutdown", _inFlightCount);
                Thread.Sleep(100);
            }

            if (_inFlightCount > 0)
            {
                _logger.LogWarning("Shutdown timeout: {Count} message(s) still in flight. Cancelling consumer.", _inFlightCount);
            }

            if (_connection.Channel?.IsOpen == true)
            {
                _connection.Channel.BasicCancel(_consumerTag!);
                _logger.LogInformation("Stopped consuming (consumer tag: {ConsumerTag})", _consumerTag);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping consumer");
        }
        finally
        {
            _consumerTag = null;
        }
    }

    private static bool IsLocalMode()
    {
        var v = Environment.GetEnvironmentVariable("TASKMANAGEMENT_LOCAL_MODE");
        return string.Equals(v, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase);
    }

    private bool EnsureConnected()
    {
        if (_connection.IsConnected && _connection.Channel != null)
            return true;
        if (!IsLocalMode())
            _logger.LogWarning("RabbitMQ is not connected. Attempting to reconnect...");
        _connection.TryConnect();
        return _connection.IsConnected && _connection.Channel != null;
    }

    public void Dispose() => _connection.Dispose();
}
