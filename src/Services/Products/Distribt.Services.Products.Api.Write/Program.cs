using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.BusinessLogic.Outbox;
using Distribt.Services.Products.BusinessLogic.Services;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddMySql<ProductsWriteStore>("distribt")
        .AddScoped<IProductsWriteStore, ProductsWriteStore>()
        .AddScoped<IUpdateProductDetails, UpdateProductDetails>()
        .AddScoped<ICreateProductDetails, CreateProductDetails>()
        .AddScoped<IStockApi,ProductsDependencyFakeType>() //testing purposes
        .AddScoped<IWarehouseApi, ProductsDependencyFakeType>() //testing purposes
        .AddScoped<IOutboxRepository, OutboxRepository>()
        .AddScoped<IOutboxProcessor, OutboxProcessor>()
        .AddHostedService<OutboxBackgroundService>()
        .AddServiceBusDomainPublisher(builder.Configuration);
});

DefaultDistribtWebApplication.Run(app);
