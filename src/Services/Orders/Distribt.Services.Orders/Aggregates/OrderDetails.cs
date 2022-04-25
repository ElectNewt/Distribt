using Distribt.Services.Orders.Dto;
using Distribt.Services.Orders.Events;
using Distribt.Shared.EventSourcing;

namespace Distribt.Services.Orders.Aggregates;

public class OrderDetails : Aggregate, IApply<OrderCreated>, IApply<OrderPaid>, IApply<OrderDispatched>, IApply<OrderCompleted>
{
    public DeliveryDetails Delivery { get; private set; } = default!;
    public PaymentInformation PaymentInformation { get; private set; } = default!;
    public List<ProductQuantity> Products { get; private set; } = new List<ProductQuantity>();
    public OrderStatus Status { get; private set; }

    public OrderDetails(Guid id) : base(id)
    {
    }

    public void Apply(OrderCreated ev)
    {
        Delivery = ev.Delivery;
        PaymentInformation = ev.PaymentInformation;
        Products = ev.Products; 
        Status = OrderStatus.Created;
        ApplyChange(ev);
    }

    public void Apply(OrderPaid ev)
    {
        Status = OrderStatus.Paid;
        ApplyChange(ev);
    }

    public void Apply(OrderDispatched ev)
    {
        Status = OrderStatus.Dispatched;
        ApplyChange(ev);
    }

    public void Apply(OrderCompleted ev)
    {
        Status = OrderStatus.Completed;
        ApplyChange(ev);
    }
}




 