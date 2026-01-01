using Domain.Orders;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seeding
{
    public sealed class DatabaseSeeder
    {
        private readonly OrderDbContext _db;

        public DatabaseSeeder(OrderDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            // Seed only once (idempotent).
            if (await _db.Orders.AnyAsync(ct))
                return;

            var order = Order.Create("Sajad Rezaiyan");
            order.AddItem("Neapolitan Pizza", 2, Money.Create(12.50m, "EUR"));
            order.AddItem("Tiramisu", 1, Money.Create(6.90m, "EUR"));
            order.Confirm();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
        }
    }
}