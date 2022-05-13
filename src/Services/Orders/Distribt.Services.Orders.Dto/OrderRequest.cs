namespace Distribt.Services.Orders.Dto;


public record CreateOrderRequest(DeliveryDetails DeliveryDetails, PaymentInformation PaymentInformation,
    List<ProductQuantity> Products);

public record CreateOrderResponse(Guid OrderId, string Location);
    
public record ProductQuantity(int ProductId, int Quantity);

public record DeliveryDetails(string Street, string City, string Country);

public record PaymentInformation(string CardNumber, string ExpireDate, string Security);




    
    