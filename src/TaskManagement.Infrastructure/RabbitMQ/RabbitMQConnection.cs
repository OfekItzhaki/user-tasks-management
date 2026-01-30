using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TaskManagement.Infrastructure.RabbitMQ;

/// <summary>
/// Manages RabbitMQ connection and channel with retry logic
/// </summary>
internal sealed class RabbitMQConnection : IDisposable
{
    private readonly ILogger<RabbitMQService> _logger;
    private readonly string _hostName;
    private IConnection? _connection;
    private IModel? _channel;

    public bool IsConnected { get; private set; }
    public IModel? Channel => _channel;

    public RabbitMQConnection(ILogger<RabbitMQService> logger, string hostName)
    {
        _logger = logger;
        _hostName = hostName;
        TryConnect();
    }

    public void TryConnect()
    {
        const int maxRetries = 5;
        int[] retryDelays = { 2000, 4000, 8000, 16000, 0 };

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                IsConnected = true;
                _logger.LogInformation("Successfully connected to RabbitMQ at {HostName} (attempt {Attempt}/{MaxRetries})", _hostName, attempt + 1, maxRetries);
                return;
            }
            catch (Exception ex)
            {
                if (attempt < maxRetries - 1)
                {
                    int delay = retryDelays[attempt];
                    _logger.LogWarning("Failed to connect to RabbitMQ at {HostName} (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms... Error: {Error}",
                        _hostName, attempt + 1, maxRetries, delay, ex.Message);
                    Thread.Sleep(delay);
                }
                else
                {
                    _logger.LogWarning(ex, "Failed to connect to RabbitMQ at {HostName} after {MaxRetries} attempts. Service will continue but reminders won't be processed. Start RabbitMQ with: docker compose -f docker/docker-compose.yml up -d rabbitmq",
                        _hostName, maxRetries);
                    IsConnected = false;
                }
            }
        }
    }

    public void MarkDisconnected() => IsConnected = false;

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
