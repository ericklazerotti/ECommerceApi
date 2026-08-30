using ECommerceApi.Application.Common.Interfaces;
using ECommerceApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Infrastructure.Persistence.Repositories;

public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public override Task<List<Category>> ListAsync(CancellationToken cancellationToken = default) =>
        DbSet.Include(c => c.Products).ToListAsync(cancellationToken);
}
