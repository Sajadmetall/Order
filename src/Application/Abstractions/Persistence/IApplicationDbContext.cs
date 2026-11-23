using Commerce.Domain.Orders;
using Commerce.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<Payment> Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
