namespace Distribt.Services.Orders.Dto;

public record OrderResponse(Guid OrderId, string orderStatus, DeliveryDetails DeliveryDetails, PaymentInformation PaymentInformation,
    List<ProductQuantityName> Products);

public record ProductQuantityName(int ProductId, int Quantity, string Name);