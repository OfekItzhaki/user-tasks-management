namespace TaskManagement.Infrastructure.RabbitMQ;

/// <summary>
/// Abstraction for publishing and consuming messages via RabbitMQ
/// </summary>
public interface IRabbitMQService
{
    void PublishMessage(string queueName, string message, IReadOnlyDictionary<string, string>? headers = null);
    /// <summary>
    /// Starts consuming. Handler returns true to ACK (success), false to Nack and requeue.
    /// </summary>
    void StartConsuming(string queueName, Func<string, bool> onMessageReceived);
    void StopConsuming();
    void Dispose();
}
