using Distribt.Services.Orders.Consumer.Handler;

WebApplication app = DefaultDistribtWebApplication.Create(x =>
{
    x.Services.AddHandlersInAssembly<OrderCreatedHandler>();
    x.Services.AddServiceBusDomainConsumer(x.Configuration);
});


DefaultDistribtWebApplication.Run(app);