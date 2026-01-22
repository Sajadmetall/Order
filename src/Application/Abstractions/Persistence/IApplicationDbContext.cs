using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
