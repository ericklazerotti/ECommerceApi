using ECommerceApi.Domain.Enums;

namespace ECommerceApi.Application.DTOs;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Total);

public record OrderDto(
    Guid Id,
    string UserId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    List<OrderItemDto> Items);

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record CreateOrderDto(List<CreateOrderItemDto> Items);
