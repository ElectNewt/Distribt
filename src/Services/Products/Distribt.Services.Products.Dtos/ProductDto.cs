namespace Distribt.Services.Products.Dtos;

public record CreateProductRequest(ProductDetails Details, int Stock, decimal Price);

public record ProductDetails(string Name, string Description);

public record FullProductResponse(int Id, ProductDetails Details, int Stock, decimal Price);

public record ProductUpdated(int ProductId, ProductDetails Details);

public record ProductCreated(int Id, CreateProductRequest ProductRequest);
