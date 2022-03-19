using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Orders.Dto;


public record OrderDto(Guid orderId, OrderAddress orderAddress, PersonalDetails PersonalDetails, List<FullProductResponse> Products);
public record CreateOrderDto(OrderAddress orderAddress, PersonalDetails PersonalDetails, List<FullProductResponse> Products);
