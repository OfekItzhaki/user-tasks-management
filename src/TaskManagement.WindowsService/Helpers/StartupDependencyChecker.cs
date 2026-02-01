using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Ensures required dependencies (SQL Server, RabbitMQ) are running before service starts.
/// In Local mode (TASKMANAGEMENT_LOCAL_MODE=1), SQL Server is checked via the actual connection string
/// (not TCP port 1433). Docker mode still uses the port check.
/// </summary>
public static class StartupDependencyChecker
{
    private static bool IsLocalMode()
    {
        var v = Environment.GetEnvironmentVariable("TASKMANAGEMENT_LOCAL_MODE");
        return string.Equals(v, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase);
    }

    public static void EnsureDependenciesRunning(ILogger logger)
    {
        var localMode = IsLocalMode();
        CheckSqlServer(logger, localMode);
        CheckRabbitMQ(logger, localMode);
    }

    private static void CheckSqlServer(ILogger logger, bool localMode)
    {
        if (localMode)
        {
            CheckSqlServerLocalMode(logger);
            return;
        }

        if (!ServiceHealthChecker.IsSqlServerRunning())
        {
            logger.LogWarning("SQL Server is not accessible on localhost:1433. Attempting to start it...");
            logger.LogWarning("Please ensure SQL Server Docker container is running: docker compose -f docker/docker-compose.yml up -d sqlserver");
            logger.LogWarning("Or if using LocalDB, ensure it's started: sqllocaldb start mssqllocaldb");
            DockerComposeStarter.TryStartSqlServer(logger);
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

    /// <summary>
    /// In Local mode we check SQL Server using the actual connection string (env or config),
    /// not TCP port 1433, since full SQL Server / LocalDB often use Named Pipes or Shared Memory.
    /// </summary>
    private static void CheckSqlServerLocalMode(ILogger logger)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogInformation("Local mode: SQL Server connection will be verified at runtime.");
            return;
        }

        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            logger.LogInformation("SQL Server is accessible");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not connect to SQL Server. Ensure it is running and your connection string is correct. Service will retry at runtime.");
        }
    }

    private static void CheckRabbitMQ(ILogger logger, bool localMode)
    {
        if (!ServiceHealthChecker.IsRabbitMQRunning())
        {
            if (localMode)
            {
                logger.LogInformation("RabbitMQ is not running. Optional in Local mode - reminder/notification features will not be available.");
            }
            else
            {
                logger.LogInformation("RabbitMQ is not running. Attempting to start it...");
                DockerComposeStarter.TryStartRabbitMQ(logger);
                if (!ServiceHealthChecker.WaitForRabbitMQReady(logger, maxWaitSeconds: 60))
                {
                    logger.LogWarning("RabbitMQ did not become ready. Service will continue but reminders won't be processed.");
                }
            }
        }
        else
        {
            logger.LogInformation("RabbitMQ is accessible");
        }
    }
}
