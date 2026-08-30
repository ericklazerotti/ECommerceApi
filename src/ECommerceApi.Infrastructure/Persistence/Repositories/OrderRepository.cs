using ECommerceApi.Application.Common.Interfaces;
using ECommerceApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Infrastructure.Persistence.Repositories;

public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<List<Order>> ListByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        DbSet.Include(o => o.Items).Where(o => o.UserId == userId).ToListAsync(cancellationToken);

    public Task<List<Order>> ListAllWithItemsAsync(CancellationToken cancellationToken = default) =>
        DbSet.Include(o => o.Items).ToListAsync(cancellationToken);
}
