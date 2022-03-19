using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Products.Api.Write.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController
{
    private readonly IUpdateProductDetails _updateProductDetails;
    private readonly ICreateProductDetails _createProductDetails;

    public ProductController(IUpdateProductDetails updateProductDetails, ICreateProductDetails createProductDetails)
    {
        _updateProductDetails = updateProductDetails;
        _createProductDetails = createProductDetails;
    }

    [HttpPost(Name = "addproduct")]
    public async Task<IActionResult> AddProduct(CreateProductRequest createProductRequest)
    {
        CreateProductResponse result = await _createProductDetails.Execute(createProductRequest);

        return new CreatedResult(new Uri(result.Url), null);
    }


    [HttpPut("updateproductdetails/{id}")]
    public async Task<IActionResult> UpdateProductDetails(int id, ProductDetails productDetails)
    {
        bool result = await _updateProductDetails.Execute(id, productDetails);

        return new OkResult();
    }
}