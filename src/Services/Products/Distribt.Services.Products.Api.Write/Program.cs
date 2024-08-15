using Distribt.Services.Products.Api.Write.Schema;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using GraphQL;
using GraphQL.Utilities;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddMySql<ProductsWriteStore>("distribt")
        .AddScoped<IProductsWriteStore, ProductsWriteStore>()
        .AddScoped<IUpdateProductDetails, UpdateProductDetails>()
        .AddScoped<ICreateProductDetails, CreateProductDetails>()
        .AddScoped<IStockApi,ProductsDependencyFakeType>() //testing purposes
        .AddScoped<IWarehouseApi, ProductsDependencyFakeType>() //testing purposes
        .AddServiceBusDomainPublisher(builder.Configuration);
    
    builder.Services.AddGraphQL(x =>
    {
        x.AddSelfActivatingSchema<ProductWriteSchema>();
        x.AddSystemTextJson();
    });
    
});


app.MapGet("graphql-schema", (ProductWriteSchema readSchema)
    =>
{
    var schemaPrinter = new SchemaPrinter(readSchema);
    return schemaPrinter.Print();
});

app.UseGraphQL<ProductWriteSchema>();

DefaultDistribtWebApplication.Run(app);


