using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.BusinessLogic;
using Distribt.Services.Orders.BusinessLogic.HealthChecks;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Services;

WebApplication app = DefaultDistribtWebApplication.Create(args, webappBuilder =>
{
    MongoMapping.RegisterClasses();
    webappBuilder.Services.AddServiceBusDomainPublisher(webappBuilder.Configuration);
    webappBuilder.Services.AddDistribtMongoDbConnectionProvider(webappBuilder.Configuration);
    webappBuilder.Services.AddEventSourcing(webappBuilder.Configuration);
    webappBuilder.Services.AddScoped<IOrderRepository, OrderRepository>();
    webappBuilder.Services.AddScoped<ICreateOrderService, CreateOrderService>();
    webappBuilder.Services.AddScoped<IGetOrderService, GetOrderService>();
    webappBuilder.Services.AddScoped<IOrderPaidService, OrderPaidService>();
    webappBuilder.Services.AddScoped<IOrderDispatchedService, OrderDispatchedService>();
    webappBuilder.Services.AddScoped<IOrderDeliveredService, OrderDeliveredService>();
    webappBuilder.Services.AddProductService(webappBuilder.Configuration);
    webappBuilder.Services.AddHealthChecks().AddCheck<ProductsHealthCheck>(nameof(ProductsHealthCheck));
});


DefaultDistribtWebApplication.Run(app);