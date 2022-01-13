using Distribt.Services.Subscriptions.Consumer.Handler;
using Distribt.Shared.Api;
using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Setup;

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