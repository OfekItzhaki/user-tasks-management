namespace TaskManagement.API.Middleware;

/// <summary>
/// Conditionally enables HTTPS redirection based on configuration
/// </summary>
public static class HttpsRedirectionMiddleware
{
    public static void UseConditionalHttpsRedirection(this IApplicationBuilder app, IConfiguration configuration)
    {
        // Only use HTTPS redirection if HTTPS port is configured
        var httpsPort = configuration.GetValue<int?>("HTTPS_PORT") ??
                        configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT") ?? 0;
        
        if (httpsPort > 0)
        {
            app.UseHttpsRedirection();
        }
    }
}
