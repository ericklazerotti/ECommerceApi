using ECommerceApi.Domain.Entities;

namespace ECommerceApi.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(List<Product> Items, int TotalCount)> ListActivePagedAsync(
        int page,
        int pageSize,
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken = default);
}
