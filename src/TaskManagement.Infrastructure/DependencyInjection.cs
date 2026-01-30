using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set it via environment variable ConnectionStrings__DefaultConnection or User Secrets. See instructions/CONFIG.md.");
        if (connectionString.Contains("***SET_VIA_ENV_OR_USER_SECRETS***", StringComparison.Ordinal))
            throw new InvalidOperationException("Replace the placeholder connection string. Set ConnectionStrings:DefaultConnection via environment variable ConnectionStrings__DefaultConnection or User Secrets. See instructions/CONFIG.md.");

        services.AddDbContext<TaskManagementDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        return services;
    }
}
