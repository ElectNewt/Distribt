using Distribt.Services.Orders.Consumer.Handler;

WebApplication app = DefaultDistribtWebApplication.Create(x =>
{
    x.Services.AddHandlers(new List<IMessageHandler>()
    {
        new OrderCreatedHandler(),
    });
    x.Services.AddServiceBusDomainConsumer(x.Configuration);
});


DefaultDistribtWebApplication.Run(app);