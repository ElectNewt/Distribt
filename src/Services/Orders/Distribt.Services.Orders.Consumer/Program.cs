using Distribt.Services.Orders.BusinessLogic;
using Distribt.Services.Orders.Consumer.Handler;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddProductService(builder.Configuration);

    builder.Services.AddHandlersInAssembly<OrderCreatedHandler>();
    builder.Services.AddServiceBusDomainConsumer(builder.Configuration);
    builder.Services.AddServiceBusIntegrationConsumer(builder.Configuration);
});


DefaultDistribtWebApplication.Run(app);