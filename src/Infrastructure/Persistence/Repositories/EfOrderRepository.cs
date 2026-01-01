
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class EfOrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _db;            

        public EfOrderRepository(OrderDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            await _db.Orders.AddAsync(order, ct);
        }

        public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default)
        {
            return await _db.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Orders
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
