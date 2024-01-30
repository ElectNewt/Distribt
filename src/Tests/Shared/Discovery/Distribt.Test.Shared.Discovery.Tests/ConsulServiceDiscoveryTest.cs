using Consul;
using Distribt.Shared.Discovery;
using Moq;
using Xunit;

namespace Distribt.Test.Shared.Discovery.Tests;

public class ConsulServiceDiscoveryTest
{

    [Fact]
    public void WhenGetFullAddressEmpty_Then_Throws()
    {
        // arrange
        var serviceKey = "my test service";

        var services = new QueryResult<CatalogService[]>()
        {
            Response = Array.Empty<CatalogService>()
        };

        var mockCatalogEndpoint = new Mock<ICatalogEndpoint>();
        mockCatalogEndpoint
            .Setup(x => x.Service(serviceKey, default))
            .ReturnsAsync(services);

        var mockClient = new Mock<IConsulClient>();
        mockClient
            .Setup(x => x.Catalog)
            .Returns(mockCatalogEndpoint.Object);

        var service = new ConsulServiceDiscovery(mockClient.Object);

        // act
        var act = async () => await service.GetFullAddress(serviceKey);

        // assert
        Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task WhenGetFullAddressAndPort_Then_GetFromClient()
    {
        // arrange
        var serviceKey = "my test service";
        var servicePort = 8765;
        var serviceAddress = "http:test.com/fake";
        var services = new QueryResult<CatalogService[]>()
        {
            Response = new[] {
                new CatalogService()
                {
                    ServiceAddress = serviceAddress,
                    ServicePort = servicePort
                }
            }
        };

        var mockCatalogEndpoint = new Mock<ICatalogEndpoint>();
        mockCatalogEndpoint
            .Setup(x => x.Service(serviceKey, default))
            .ReturnsAsync(services);

        var mockClient = new Mock<IConsulClient>();
        mockClient
            .Setup(x => x.Catalog)
            .Returns(mockCatalogEndpoint.Object);

        var service = new ConsulServiceDiscovery(mockClient.Object);

        // act
        var result = await service.GetFullAddress(serviceKey);

        // assert
        Assert.Equal($"{serviceAddress}:{servicePort}", result);
    }

    [Fact]
    public async Task WhenGetFullAddressAndNoPort_Then_GetFromClient()
    {
        // arrange
        var serviceKey = "my test service";
        var servicePort = 0;
        var serviceAddress = "http:test.com/fake";
        var services = new QueryResult<CatalogService[]>()
        {
            Response = new[] {
                new CatalogService()
                {
                    ServiceAddress = serviceAddress,
                    ServicePort = servicePort
                }
            }
        };

        var mockCatalogEndpoint = new Mock<ICatalogEndpoint>();
        mockCatalogEndpoint
            .Setup(x => x.Service(serviceKey, default))
            .ReturnsAsync(services);

        var mockClient = new Mock<IConsulClient>();
        mockClient
            .Setup(x => x.Catalog)
            .Returns(mockCatalogEndpoint.Object);

        var service = new ConsulServiceDiscovery(mockClient.Object);

        // act
        var result = await service.GetFullAddress(serviceKey);

        // assert
        Assert.Equal($"{serviceAddress}", result);
    }

    [Fact]
    public async Task WhenGetFullAddress_Then_UseCacheValue()
    {
        // arrange
        var serviceKey = "my test service";
        var servicePort = 0;
        var serviceAddress = "http:test.com/fake";
        var services = new QueryResult<CatalogService[]>()
        {
            Response = new[] {
                new CatalogService()
                {
                    ServiceAddress = serviceAddress,
                    ServicePort = servicePort
                }
            }
        };

        var mockCatalogEndpoint = new Mock<ICatalogEndpoint>();
        mockCatalogEndpoint
            .Setup(x => x.Service(serviceKey, default))
            .ReturnsAsync(services);

        var mockClient = new Mock<IConsulClient>();
        mockClient
            .Setup(x => x.Catalog)
            .Returns(mockCatalogEndpoint.Object);

        var service = new ConsulServiceDiscovery(mockClient.Object);

        // act
        await service.GetFullAddress(serviceKey);
        await service.GetFullAddress(serviceKey);

        // assert
        mockCatalogEndpoint
            .Verify(x => x.Service(serviceKey, default), Times.Once);
    }
}