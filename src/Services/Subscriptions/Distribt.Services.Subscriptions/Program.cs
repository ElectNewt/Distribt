WebApplication app = DefaultDistribtWebApplication.Create(args, webappBuilder =>
{
    webappBuilder.Services.AddServiceBusIntegrationPublisher(webappBuilder.Configuration);
});

DefaultDistribtWebApplication.Run(app);

public partial class Program { }