using TaskManagement.Application;
using TaskManagement.Infrastructure;
using TaskManagement.API.Configuration;
using TaskManagement.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTaskManagementApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure middleware pipeline (order matters)
app.UseMiddleware<Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<Middleware.RequestResponseLoggingMiddleware>();
app.UseMiddleware<Middleware.SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseConditionalHttpsRedirection(app.Configuration);
app.UseCorsPolicy();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
