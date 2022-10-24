using Microsoft.Extensions.Configuration;

namespace Distribt.Shared.Setup.API.Key;

public static class ApiKeyDependencyInjection
{
    public static IServiceCollection AddApiToken(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<ApiKeyConfiguration>(configuration.GetSection("ApiKey"));
    }

    public static void UseApiTokenMiddleware(this WebApplication webApp)
    {
        //Do not act on /health or /health-ui
        webApp.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"),
            appBuilder => appBuilder.UseMiddleware<ApiKeyMiddleware>()
        );
    }
}