using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Distribt.Shared.Api;

public static class DefaultDistribtWebApplication
{
    public static WebApplication Create(Action<WebApplicationBuilder>? webappBuilder = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        if (webappBuilder != null)
        {
            webappBuilder.Invoke(builder);

        }

        return builder.Build();
    }

    public static void Run(WebApplication webApp)
    {
        if (webApp.Environment.IsDevelopment())
        {
            webApp.UseSwagger();
            webApp.UseSwaggerUI();
        }


        webApp.UseHttpsRedirection();

        webApp.UseAuthorization();

        webApp.MapControllers();

        webApp.Run();
    }
}

