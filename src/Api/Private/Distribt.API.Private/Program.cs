using Distribt.Shared.Setup.API.Key;
using Distribt.Shared.Setup.API.RateLimiting;

WebApplication app = await DefaultDistribtWebApplication.Create(args, webappBuilder =>
{
    webappBuilder.Services.AddReverseProxy()
        .LoadFromConfig(webappBuilder.Configuration.GetSection("ReverseProxy"));

    webappBuilder.Services.AddApiToken(webappBuilder.Configuration);
    webappBuilder.Services.AddRateLimiter(o => { });
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