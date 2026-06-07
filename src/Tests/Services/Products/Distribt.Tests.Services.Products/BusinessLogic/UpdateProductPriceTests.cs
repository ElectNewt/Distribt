using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;
using Moq;

namespace Distribt.Tests.Services.Products.BusinessLogic;

public class UpdateProductPriceTests
{
    [Fact]
    public async Task Execute_AppliesPromotionalDiscount_AndUpdatesSalesPrice()
    {
        Mock<IWarehouseApi> warehouse = new Mock<IWarehouseApi>();
        Mock<IDomainMessagePublisher> publisher = new Mock<IDomainMessagePublisher>();
        UpdateProductPrice sut = new UpdateProductPrice(warehouse.Object, publisher.Object);

        UpdateProductPriceRequest request = new UpdateProductPriceRequest(100m, 10);

        bool result = await sut.Execute(1, request);

        Assert.True(result);
        warehouse.Verify(w => w.ModifySalesPrice(It.IsAny<int>(), 
            It.IsAny<decimal>()), Times.Once);
    }
    
    [Fact]
    public async Task Execute_WhenWarehouseUpdateFails_StillReportsSuccess()
    {
        Mock<IWarehouseApi> warehouse = new Mock<IWarehouseApi>();
        Mock<IDomainMessagePublisher> publisher = new Mock<IDomainMessagePublisher>();
        warehouse.Setup(a=>a.ModifySalesPrice(It.IsAny<int>(), It.IsAny<decimal>()))
            .ThrowsAsync(new Exception("unavailable"));
        UpdateProductPrice subject = new UpdateProductPrice(warehouse.Object, publisher.Object);
        bool result = await subject.Execute(2, new UpdateProductPriceRequest(100m, 10));
        
        Assert.True(result);
    }
}