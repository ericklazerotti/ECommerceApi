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

    public Task<List<Product>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        DbSet.Include(p => p.Category).Where(p => p.IsActive).ToListAsync(cancellationToken);
}
