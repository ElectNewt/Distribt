using Distribt.Services.Orders.Dto;

namespace Distribt.Services.Orders.Events;

public record OrderCreated(DeliveryDetails Delivery, PaymentInformation PaymentInformation,
    List<ProductQuantity> Products);

public record OrderPaid();

public record OrderDispatched();

public record OrderCompleted();


public enum OrderStatus
{
    Created, 
    Paid,
    Dispatched,
    Completed,
    Failed
}
