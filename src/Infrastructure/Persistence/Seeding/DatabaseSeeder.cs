using Commerce.Domain.Common;
using Commerce.Domain.Orders;
using Commerce.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Infrastructure.Persistence.Seeding
{
    public sealed class DatabaseSeeder
    {
        private readonly OrderDbContext _db;
        private readonly TimeProvider _timeProvider;

        public DatabaseSeeder(OrderDbContext db, TimeProvider timeProvider)
        {
            _db = db;
            _timeProvider = timeProvider;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            await AddExampleAsync("ee71390b-9765-4c56-8fea-044cb2f93a01", "Demo - Draft", 1, 4.90m, ct);
            await AddExampleAsync("ee71390b-9765-4c56-8fea-044cb2f93a02", "Demo - Prepared payment", 2, 4.90m, ct,
                PreparePayment);
            await AddExampleAsync("ee71390b-9765-4c56-8fea-044cb2f93a03", "Demo - Pending payment", 1, 9.90m, ct,
                order =>
                {
                    var payment = PreparePayment(order);
                    payment.StartAttempt();
                    return payment;
                });
            await AddExampleAsync("ee71390b-9765-4c56-8fea-044cb2f93a04", "Demo - Failed payment", 3, 4.90m, ct,
                order =>
                {
                    var payment = PreparePayment(order);
                    payment.RecordFailure(payment.StartAttempt());
                    return payment;
                });
            await AddExampleAsync("ee71390b-9765-4c56-8fea-044cb2f93a05", "Demo - Paid after retry", 2, 9.90m, ct,
                order =>
                {
                    var payment = PreparePayment(order);
                    payment.RecordFailure(payment.StartAttempt());
                    payment.RecordSuccess(payment.StartAttempt(), payment.Amount, $"seed-{order.Id.Value:N}");
                    order.MarkPaid(payment.Amount);
                    return payment;
                });

            // Persist the example state without publishing a purchase event for a demo order.
            await _db.SaveChangesAsync(ct);
        }

        private async Task AddExampleAsync(string id, string customerName, int quantity, decimal unitPrice,
            CancellationToken ct, Func<Order, Payment>? createPayment = null)
        {
            var seedId = new OrderId(Guid.Parse(id));
            if (await _db.Orders.AnyAsync(order => order.Id == seedId, ct))
                return;

            var order = Order.Create(customerName, _timeProvider.GetUtcNow());
            // The fixed seed identity allows independent additions without matching customer data.
            _db.Entry(order).Property(order => order.Id).CurrentValue = seedId;
            order.AddItem("Premium learning period", quantity, Money.Create(unitPrice, "EUR"));

            var payment = createPayment?.Invoke(order);
            _db.Orders.Add(order);
            if (payment is not null)
                _db.Payments.Add(payment);
        }

        private static Payment PreparePayment(Order order)
        {
            order.Confirm();
            return Payment.Create(order.Id, order.GetTotal());
        }
    }
}
