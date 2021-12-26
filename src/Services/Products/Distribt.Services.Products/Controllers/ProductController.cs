using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Products.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController
{
    [HttpGet("{productId}")]
    public Task<ProductDto> GetProduct(Guid productId)
    {
        return Task.FromResult(new ProductDto(productId, "TODO", "TODO"));
    }

    [HttpPost(Name = "addproduct")]
    public Task<bool> GetProduct(ProductDto product)
    {
        return Task.FromResult(true);
    }
}

public record ProductDto(Guid productId, string productName, string productDescription);

