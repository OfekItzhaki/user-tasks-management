using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Checks if external services (RabbitMQ, SQL Server) are accessible
/// </summary>
public static class ServiceHealthChecker
{
    public static bool IsRabbitMQRunning()
    {
        return IsServiceRunning("localhost", 5672);
    }

    public static bool IsSqlServerRunning()
    {
        return IsServiceRunning("localhost", 1433);
    }

    public static bool WaitForRabbitMQReady(ILogger logger, int maxWaitSeconds = 60)
    {
        return WaitForServiceReady(logger, "RabbitMQ", "localhost", 5672, maxWaitSeconds);
    }

    public static bool WaitForSqlServerReady(ILogger logger, int maxWaitSeconds = 60)
    {
        return WaitForServiceReady(logger, "SQL Server", "localhost", 1433, maxWaitSeconds);
    }

    private static bool IsServiceRunning(string host, int port)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port);
            if (connectTask.Wait(TimeSpan.FromSeconds(2)))
            {
                return tcpClient.Connected;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool WaitForServiceReady(ILogger logger, string serviceName, string host, int port, int maxWaitSeconds)
    {
        logger.LogInformation("Waiting for {ServiceName} to be ready on {Host}:{Port}...", serviceName, host, port);
        int waited = 0;
        int checkInterval = 2; // Check every 2 seconds

        while (waited < maxWaitSeconds)
        {
            if (IsServiceRunning(host, port))
            {
                logger.LogInformation("{ServiceName} is ready (waited {Waited} seconds)", serviceName, waited);
                return true;
            }

            Thread.Sleep(checkInterval * 1000);
            waited += checkInterval;

            // Log progress every 10 seconds
            if (waited % 10 == 0)
            {
                logger.LogInformation("Still waiting for {ServiceName}... ({Waited}/{MaxWaitSeconds} seconds)", serviceName, waited, maxWaitSeconds);
            }
        }

        logger.LogWarning("{ServiceName} did not become ready within {MaxWaitSeconds} seconds", serviceName, maxWaitSeconds);
        return false;
    }
}
