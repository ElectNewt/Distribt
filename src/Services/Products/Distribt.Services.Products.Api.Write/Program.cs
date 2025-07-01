using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Shared.Outbox.Processor;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddMySql<ProductsWriteStore>("distribt")
        .AddScoped<IProductsWriteStore, ProductsWriteStore>()
        .AddScoped<IUpdateProductDetails, UpdateProductDetails>()
        .AddScoped<ICreateProductDetails, CreateProductDetails>()
        .AddScoped<IStockApi, ProductsDependencyFakeType>() //testing purposes
        .AddScoped<IWarehouseApi, ProductsDependencyFakeType>() //testing purposes
        .AddServiceBusDomainPublisher(builder.Configuration);
        builder.Services.AddHostedService<OutboxProcessor<ProductsWriteStore>>();
});

