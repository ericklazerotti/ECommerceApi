using ECommerceApi.Application.DTOs;

namespace ECommerceApi.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(string userId, CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> ListByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> MarkAsShippedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
