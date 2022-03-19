using Distribt.Services.Subscriptions.Consumer.Handler;

WebApplication app = DefaultDistribtWebApplication.Create(args, x =>
{
    x.Services.AddScoped<IDependenciaTest, DependenciaTest>();
    x.Services.AddHandlersInAssembly<SubscriptionHandler>();
    x.Services.AddServiceBusIntegrationConsumer(x.Configuration);
});


DefaultDistribtWebApplication.Run(app);