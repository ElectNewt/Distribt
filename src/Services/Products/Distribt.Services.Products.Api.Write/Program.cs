using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.BusinessLogic.BackgroundServices;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddMySql<ProductsWriteStore>("distribt")
        .AddScoped<IProductsWriteStore, ProductsWriteStore>()
        .AddScoped<IUpdateProductDetails, UpdateProductDetailsWithOutbox>()
        .AddScoped<ICreateProductDetails, CreateProductDetailsWithOutbox>()
        .AddScoped<IOutboxRepository, OutboxRepository>()
        .AddScoped<IOutboxMessageService, OutboxMessageService>()
        .AddScoped<IStockApi,ProductsDependencyFakeType>() //testing purposes
        .AddScoped<IWarehouseApi, ProductsDependencyFakeType>() //testing purposes
        .AddServiceBusDomainPublisher(builder.Configuration);
        builder.Services
        .AddHostedService<OutboxProcessorService>();
});

DefaultDistribtWebApplication.Run(app);
