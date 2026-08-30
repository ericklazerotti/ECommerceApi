using ECommerceApi.Domain.Entities;

namespace ECommerceApi.Application.Common.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Order>> ListByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Order>> ListAllWithItemsAsync(CancellationToken cancellationToken = default);
}
