using Distribt.Services.Orders.Dto;
using Distribt.Services.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Orders.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController
{
    private readonly ICreateOrderService _createOrderService;
    private readonly IGetOrderService _getOrderService;
    private readonly IOrderPaidService _orderPaidService;
    private readonly IOrderDispatchedService _orderDispatchedService;
    

    public OrderController(ICreateOrderService createOrderService,
        IGetOrderService getOrderService, IOrderPaidService orderPaidService, IOrderDispatchedService orderDispatchedService)
    {
        _createOrderService = createOrderService;
        _getOrderService = getOrderService;
        _orderPaidService = orderPaidService;
        _orderDispatchedService = orderDispatchedService;
    }

    [HttpGet("{orderId}")]
    public async Task<OrderResponse> GetOrder(Guid orderId)
        => await _getOrderService.Execute(orderId);


    [HttpGet("getorderstatus/{orderId}")]
    public Task<OrderResponse> GetOrderStatus(Guid orderId)
    {
        throw new NotImplementedException();
    }

    [HttpPost("create")]
    public async Task<ActionResult<Guid>> CreateOrder(CreateOrderRequest createOrderRequest,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        Guid orderId = await _createOrderService.Execute(createOrderRequest, cancellationToken);
        return new AcceptedResult($"getorderstatus/{orderId}", orderId);
    }

    [HttpPut("markaspaid")]
    public async Task OrderPaid(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
        => await _orderPaidService.Execute(orderId, cancellationToken);

    [HttpPut("markasdispatched")]
    public async Task OrderDispatched(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
        => await _orderDispatchedService.Execute(orderId, cancellationToken);
    
    [HttpPut("markasdelivered")]
    public async Task OrderDelivered(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
        => await _orderDispatchedService.Execute(orderId, cancellationToken);
}