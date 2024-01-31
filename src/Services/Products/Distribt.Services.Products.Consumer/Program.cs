using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Consumer.Handlers;
using Distribt.Shared.Databases.MongoDb;
using Microsoft.Extensions.Options;

WebApplication app = await DefaultDistribtWebApplication.Create(args, async builder =>
{
    (await builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration))
        .AddScoped< IProductsReadStore>((IServiceProvider serviceProvider) =>
        {
            var mongoDbConnectionProvider = serviceProvider.GetService<IMongoDbConnectionProvider>() ?? throw new ArgumentNullException();
            var databaseConfiguration = serviceProvider.GetService<IOptions<DatabaseConfiguration>>() ?? throw new ArgumentNullException();
            var mongoUrl = mongoDbConnectionProvider.GetMongoUrl().GetAwaiter().GetResult();
            return new ProductsReadStore(mongoUrl, databaseConfiguration);
        });
    builder.Services.AddServiceBusIntegrationPublisher(builder.Configuration);
    builder.Services.AddHandlersInAssembly<ProductUpdatedHandler>();
    builder.Services.AddServiceBusDomainConsumer(builder.Configuration);
});


DefaultDistribtWebApplication.Run(app);