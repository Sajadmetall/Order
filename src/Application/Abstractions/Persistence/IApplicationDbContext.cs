using Commerce.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
