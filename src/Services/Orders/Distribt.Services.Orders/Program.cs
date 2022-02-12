WebApplication app = DefaultDistribtWebApplication.Create(webappBuilder =>
{
    webappBuilder.Services.AddServiceBusDomainPublisher(webappBuilder.Configuration);
    
});


DefaultDistribtWebApplication.Run(app);