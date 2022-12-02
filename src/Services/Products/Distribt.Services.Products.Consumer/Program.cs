using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Consumer.Handlers;
using Distribt.Shared.Databases.MongoDb;
using Microsoft.Extensions.Options;

WebApplication app = await DefaultDistribtWebApplication.Create(args, async builder =>
{
    var factory = async (IServiceProvider serviceProvider) =>
    {
        var mongoDbConnectionProvider = serviceProvider.GetService<IMongoDbConnectionProvider>() ?? throw new ArgumentNullException();
        var databaseConfiguration = serviceProvider.GetService<IOptions<DatabaseConfiguration>>() ?? throw new ArgumentNullException();
        var mongoUrl = await mongoDbConnectionProvider.GetMongoUrl();
        return new ProductsReadStore(mongoUrl, databaseConfiguration);
    };
    (await builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration))
        .AddScoped(typeof(IProductsReadStore), factory);
    builder.Services.AddServiceBusIntegrationPublisher(builder.Configuration);
    builder.Services.AddHandlersInAssembly<ProductUpdatedHandler>();
    builder.Services.AddServiceBusDomainConsumer(builder.Configuration);
});


DefaultDistribtWebApplication.Run(app);