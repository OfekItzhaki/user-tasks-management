using TaskManagement.Application;
using TaskManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection if HTTPS port is configured
var httpsPort = app.Configuration.GetValue<int?>("HTTPS_PORT") ?? 
                app.Configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT") ?? 0;
if (httpsPort > 0)
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
