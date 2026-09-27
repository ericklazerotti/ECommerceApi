using ECommerceApi.Application.Common.Interfaces;
using ECommerceApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Infrastructure.Persistence.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<Product?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(List<Product> Items, int TotalCount)> ListActivePagedAsync(
        int page,
        int pageSize,
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(p => p.Category).Where(p => p.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // ToLower().Contains() em vez de EF.Functions.ILike: traduz tanto no
            // Postgres quanto no provider InMemory usado nos testes de unidade.
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
