using Distribt.Services.Subscriptions.Consumer.Handler;

WebApplication app = DefaultDistribtWebApplication.Create(x =>
{
    x.Services.AddHandlers(new List<IMessageHandler>()
    {
        new SubscriptionHandler(),
        new UnSubscriptionHandler()
    });
    x.Services.AddServiceBusIntegrationConsumer(x.Configuration);
});


DefaultDistribtWebApplication.Run(app);