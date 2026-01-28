using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using TaskManagement.WindowsService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure console logging with better formatting
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "simple";
});
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

builder.Services.AddSingleton<IRabbitMQService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQService>>();
    var hostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
    return new RabbitMQService(logger, hostName);
});

builder.Services.AddHostedService<TaskReminderService>();

var host = builder.Build();

host.Run();
