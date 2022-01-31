using Distribt.Shared.Communication.RabbitMQ;
using Distribt.Shared.Secrets;

WebApplication app = DefaultDistribtWebApplication.Create(webappBuilder =>
{
    webappBuilder.Services.AddReverseProxy()
        .LoadFromConfig(webappBuilder.Configuration.GetSection("ReverseProxy"));
});
app.MapGet("/", () => "Hello World!");

app.MapReverseProxy();
DefaultDistribtWebApplication.Run(app);
