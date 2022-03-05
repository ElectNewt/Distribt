WebApplication app = DefaultDistribtWebApplication.Create(args, webappBuilder =>
{
    webappBuilder.Services.AddServiceBusDomainPublisher(webappBuilder.Configuration);
    
});


DefaultDistribtWebApplication.Run(app);