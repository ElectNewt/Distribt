using Distribt.Shared.Setup.API.Key;

WebApplication app = DefaultDistribtWebApplication.Create(args, webappBuilder =>
{
    webappBuilder.Services.AddReverseProxy()
        .LoadFromConfig(webappBuilder.Configuration.GetSection("ReverseProxy"));
    
    webappBuilder.Services.AddApiToken(webappBuilder.Configuration);
  
});
app.MapGet("/", () => "Hello World!");

app.MapReverseProxy();
app.UseApiTokenMiddleware();
DefaultDistribtWebApplication.Run(app);
