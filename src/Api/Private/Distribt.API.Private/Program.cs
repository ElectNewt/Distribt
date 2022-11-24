using Distribt.Shared.Setup.API.Key;
using Distribt.Shared.Setup.API.RateLimiting;

WebApplication app = DefaultDistribtWebApplication.Create(args, webappBuilder =>
{
    webappBuilder.Services.AddReverseProxy()
        .LoadFromConfig(webappBuilder.Configuration.GetSection("ReverseProxy"));

    webappBuilder.Services.AddApiToken(webappBuilder.Configuration);
});

app.UseApiTokenMiddleware();
app.UseRateLimiter();
app.MapGet("/", () => "Hello World!");
app.MapGet("/rate-limiting-test", () =>
{
    return "Hello World!";
}).RequireRateLimiting(new DistribtRateLimiterPolicy());

app.MapReverseProxy();

DefaultDistribtWebApplication.Run(app);