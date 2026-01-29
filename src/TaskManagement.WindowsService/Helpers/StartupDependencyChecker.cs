using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Ensures required dependencies (SQL Server, RabbitMQ) are running before service starts
/// </summary>
public static class StartupDependencyChecker
{
    public static void EnsureDependenciesRunning(ILogger logger)
    {
        CheckSqlServer(logger);
        CheckRabbitMQ(logger);
    }

    private static void CheckSqlServer(ILogger logger)
    {
        if (!ServiceHealthChecker.IsSqlServerRunning())
        {
            logger.LogWarning("SQL Server is not accessible on localhost:1433. Attempting to start it...");
            logger.LogWarning("Please ensure SQL Server Docker container is running: docker compose -f docker/docker-compose.yml up -d sqlserver");
            logger.LogWarning("Or if using LocalDB, ensure it's started: sqllocaldb start mssqllocaldb");

            // Try to start SQL Server container
            DockerComposeStarter.TryStartSqlServer(logger);

            // Wait for SQL Server to be ready (critical - service needs database)
            // SQL Server can take 30-60 seconds to fully initialize
            if (!ServiceHealthChecker.WaitForSqlServerReady(logger, maxWaitSeconds: 90))
            {
                logger.LogError("SQL Server did not become ready. Service may not function correctly.");
            }
        }
        else
        {
            logger.LogInformation("SQL Server is accessible");
        }
    }

    private static void CheckRabbitMQ(ILogger logger)
    {
        if (!ServiceHealthChecker.IsRabbitMQRunning())
        {
            logger.LogInformation("RabbitMQ is not running. Attempting to start it...");
            DockerComposeStarter.TryStartRabbitMQ(logger);

            // Wait for RabbitMQ to be ready after starting
            if (!ServiceHealthChecker.WaitForRabbitMQReady(logger, maxWaitSeconds: 60))
            {
                logger.LogWarning("RabbitMQ did not become ready. Service will continue but reminders won't be processed.");
            }
        }
        else
        {
            logger.LogInformation("RabbitMQ is accessible");
        }
    }
}
