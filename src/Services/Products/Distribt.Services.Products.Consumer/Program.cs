using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Consumer.Handlers;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration)
        .AddScoped<IProductsReadStore, ProductsReadStore>();
    builder.Services.AddServiceBusIntegrationPublisher(builder.Configuration);
    builder.Services.AddHandlersInAssembly<ProductUpdatedHandler>();
    builder.Services.AddServiceBusDomainConsumer(builder.Configuration);
});


DefaultDistribtWebApplication.Run(app);