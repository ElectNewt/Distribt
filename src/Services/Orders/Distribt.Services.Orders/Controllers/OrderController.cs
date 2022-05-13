using System.Net;
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
        IGetOrderService getOrderService, IOrderPaidService orderPaidService,
        IOrderDispatchedService orderDispatchedService)
    {
        _createOrderService = createOrderService;
        _getOrderService = getOrderService;
        _orderPaidService = orderPaidService;
        _orderDispatchedService = orderDispatchedService;
    }

    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(ResultDto<OrderResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ResultDto<OrderResponse>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetOrder(Guid orderId)
        => await _getOrderService.Execute(orderId)
            .UseSuccessHttpStatusCode(HttpStatusCode.OK)
            .ToActionResult();


    [HttpGet("getorderstatus/{orderId}")]
    public Task<OrderResponse> GetOrderStatus(Guid orderId)
    {
        throw new NotImplementedException();
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(ResultDto<CreateOrderResponse>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest createOrderRequest,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await _createOrderService.Execute(createOrderRequest, cancellationToken)
            .UseSuccessHttpStatusCode(HttpStatusCode.Created)
            .ToActionResult();
    }

    [HttpPut("markaspaid")]
    [ProducesResponseType(typeof(ResultDto<bool>), (int)HttpStatusCode.Accepted)]
    public async Task<IActionResult> OrderPaid(Guid orderId,
        CancellationToken cancellationToken = default(CancellationToken))
        => await _orderPaidService.Execute(orderId, cancellationToken)
            .Success().Async().ToActionResult();

    [HttpPut("markasdispatched")]
    [ProducesResponseType(typeof(ResultDto<bool>), (int)HttpStatusCode.Accepted)]
    public async Task OrderDispatched(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
        => await _orderDispatchedService.Execute(orderId, cancellationToken)
            .Success().Async().ToActionResult();

    [HttpPut("markasdelivered")]
    [ProducesResponseType(typeof(ResultDto<bool>), (int)HttpStatusCode.Accepted)]
    public async Task OrderDelivered(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
        => await _orderDispatchedService.Execute(orderId, cancellationToken)
            .Success().Async().ToActionResult();
}