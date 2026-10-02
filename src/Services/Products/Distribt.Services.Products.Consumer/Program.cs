using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Consumer.Handlers;
using Distribt.Shared.Databases.MongoDb;
using Microsoft.Extensions.Options;

WebApplication app = await DefaultDistribtWebApplication.Create(args, async builder =>
{
    await builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration);
    builder.Services.AddScoped<IProductsReadStore, ProductsReadStore>();
    builder.Services.AddServiceBusIntegrationPublisher(builder.Configuration);
    builder.Services.AddHandlersInAssembly<ProductUpdatedHandler>();
    builder.Services.AddServiceBusDomainConsumer(builder.Configuration);
});


DefaultDistribtWebApplication.Run(app);