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

            // NOTE: Replace OrderItem ctor / factory based on your actual OrderItem implementation.
            var order1 = new Order(
                customerName: "Sajad Rezaiyan",
                items: new[]
                {
                // Example: new OrderItem(productName, quantity, unitPrice)
                new OrderItem("Neapolitan Pizza", 2, Money.Create(12.50m, "EUR")),
                new OrderItem("Tiramisu", 1, Money.Create(6.90m, "EUR")),
                }
            );
            // Seed only once (idempotent).
            if (await _db.Orders.AnyAsync(ct))
                return;

            var order1 = new Order(
                customerName: "Sajad Rezaiyan",
                items: new[]
                {
                OrderItem.Create("Neapolitan Pizza", 2, Money.Create(12.50m, "EUR")),
                OrderItem.Create("Tiramisu", 1, Money.Create(6.90m, "EUR")),
                }
            );

            order1.Confirm();

            var order2 = new Order(
                customerName: "John Doe",
                items: new[]
                {
                OrderItem.Create("Espresso", 3, Money.Create(2.20m, "EUR")),
                }
            );

            order2.Confirm();
            order2.Ship();

            await _db.Orders.AddRangeAsync(order1, order2, ct);
            await _db.SaveChangesAsync(ct);
            order1.Confirm();

            var order2 = new Order(
                customerName: "John Doe",
                items: new[]
                {
                new OrderItem("Espresso", 3, Money.Create(2.20m, "EUR")),
                }
            );

            order2.Confirm();
            order2.Ship();

            await _db.Orders.AddRangeAsync(order1, order2, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}