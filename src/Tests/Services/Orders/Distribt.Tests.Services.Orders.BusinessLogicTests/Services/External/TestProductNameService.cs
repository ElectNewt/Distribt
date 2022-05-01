using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Distribt.Services.Orders.BusinessLogic.Data.External;
using Distribt.Services.Orders.BusinessLogic.Services.External;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Discovery;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Distribt.Tests.Services.Orders.BusinessLogic.Services.External;

public class TestProductNameService
{
    public class TestState
    {
        public Mock<IProductRepository> ProductRepository { get; set; }
        public Mock<IDistributedCache> Cache { get; set; }
        public Mock<IHttpClientFactory> ClientFactory { get; set; }
        public Mock<IServiceDiscovery> ServiceDiscovery { get; set; }
        public ProductNameService Subject { get; set; }

        public TestState()
        {
            ProductRepository = new Mock<IProductRepository>();
            Cache = new Mock<IDistributedCache>();
            ClientFactory = new Mock<IHttpClientFactory>();
            ServiceDiscovery = new Mock<IServiceDiscovery>();


            Cache.Setup(a => a.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as byte[]);

            ProductRepository.Setup(a => a.GetProductName(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as string);

            ServiceDiscovery.Setup(a =>
                    a.GetFullAddress(DiscoveryServices.Microservices.ProductsApi.ApiRead,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync("http://productApi");

            Subject = new ProductNameService(ProductRepository.Object, Cache.Object, ClientFactory.Object,
                ServiceDiscovery.Object);
        }
    }


    [Fact]
    public async Task WhenProductNameOnCache_ThenRetrieveFromCache()
    {
        int id = 1;
        string expected = "returnedValue";
        TestState state = new TestState();

        state.Cache.Setup(a => a.GetAsync($"ORDERS-PRODUCT::{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.ASCII.GetBytes(expected));

        string result = await state.Subject.GetProductName(id);

        Assert.Equal(expected, result);
        state.Cache.Verify(a => a.GetAsync($"ORDERS-PRODUCT::{id}", It.IsAny<CancellationToken>()), Times.Once);
        state.ProductRepository.Verify(a => a.GetProductName(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        state.ServiceDiscovery.Verify(a =>
            a.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenProductNameNotInCache_ThenGetItFromDatabase()
    {
        int id = 1;
        string expected = "returnedValue";
        TestState state = new TestState();
        state.ProductRepository.Setup(a => a.GetProductName(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        string result = await state.Subject.GetProductName(id);

        Assert.Equal(expected, result);
        state.Cache.Verify(a => a.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        state.ProductRepository.Verify(a => a.GetProductName(id, It.IsAny<CancellationToken>()), Times.Once);
        state.ServiceDiscovery.Verify(a =>
            a.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenProductNameNotInCacheOrDb_ThenCallOtherMicroservice_AndSetProductName()
    {
        int id = 1;
        string expected = "returnedValue";
        TestState state = new TestState();

        state.ClientFactory.Setup(a => a.CreateClient(It.IsAny<string>()))
            .Returns(FakeHttpClient(id, expected));

        string result = await state.Subject.GetProductName(id);

        Assert.Equal(expected, result);
        state.Cache.Verify(a => a.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        state.ProductRepository.Verify(a => a.GetProductName(id, It.IsAny<CancellationToken>()), Times.Once);
        state.ServiceDiscovery.Verify(a =>
            a.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        state.ProductRepository.Verify(a => a.UpsertProductName(id, expected, It.IsAny<CancellationToken>()),
            Times.Once);
        state.Cache.Verify(
            a => a.SetAsync($"ORDERS-PRODUCT::{id}", Encoding.ASCII.GetBytes(expected),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    
    [Fact]
    public async Task WhenSetProductName_ThenUpsertInDbAndSetInCache()
    {
        int id = 1;
        string expected = "returnedValue";
        TestState state = new TestState();

        await state.Subject.SetProductName(id, expected);

        state.ProductRepository.Verify(a => a.UpsertProductName(id, expected, It.IsAny<CancellationToken>()),
            Times.Once);
        state.Cache.Verify(
            a => a.SetAsync($"ORDERS-PRODUCT::{id}", Encoding.ASCII.GetBytes(expected),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private HttpClient FakeHttpClient(int id, string name)
    {
        FullProductResponse productResponse = new FullProductResponse(id, new ProductDetails(name, "random"), 0, 0);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(productResponse))
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        return client;
    }
}