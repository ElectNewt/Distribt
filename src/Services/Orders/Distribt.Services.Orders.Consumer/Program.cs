using Distribt.Services.Orders.BusinessLogic;
using Distribt.Services.Orders.Consumer.Handler;

WebApplication app = await DefaultDistribtWebApplication.Create(args, async builder =>
{
    await builder.Services.AddProductService(builder.Configuration);

    builder.Services.AddHandlersInAssembly<OrderCreatedHandler>();
    builder.Services.AddServiceBusDomainConsumer(builder.Configuration);
    builder.Services.AddServiceBusIntegrationConsumer(builder.Configuration);
});


DefaultDistribtWebApplication.Run(app);