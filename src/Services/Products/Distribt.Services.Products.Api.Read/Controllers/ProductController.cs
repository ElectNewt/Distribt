// using Distribt.Services.Products.BusinessLogic.DataAccess;
// using Distribt.Services.Products.Dtos;
// using Microsoft.AspNetCore.Mvc;
//
// namespace Distribt.Services.Products.Api.Read.Controllers;
//
//
// [ApiController]
// [Route("[controller]")]
// public class ProductController
// {
//     private readonly IProductsReadStore _productsReadStore;
//
//     public ProductController(IProductsReadStore productsReadStore)
//     {
//         _productsReadStore = productsReadStore;
//     }
//
//     [HttpGet("{productId}")]
//     public async Task<FullProductResponse> GetProduct(int productId)
//     {
//         return await _productsReadStore.GetFullProduct(productId);
//     }
// }