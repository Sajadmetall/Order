using Commerce.Application.Features.Payments;
using Commerce.Domain.Orders;
using Commerce.Domain.Payments;
using Commerce.Infrastructure.Persistence;
using Commerce.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Commerce.Integration.Tests.Workflows;

[Trait("Category", "Integration")]
public sealed class PersistenceSafetyTests(CommerceFixture infrastructure)
    : PurchaseTestBase(infrastructure), IClassFixture<CommerceFixture>
{
    [Fact]
    public async Task A_stale_writer_cannot_create_a_second_pending_attempt()
    {
        var orderId = await CreateOrderAsync();
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        var id = new PaymentId(paymentId);
        await using var firstScope = Factory.Services.CreateAsyncScope();
        await using var secondScope = Factory.Services.CreateAsyncScope();
        var firstContext = firstScope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var secondContext = secondScope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var first = await firstContext.Payments.Include(payment => payment.Attempts)
            .SingleAsync(payment => payment.Id == id);
        var stale = await secondContext.Payments.Include(payment => payment.Attempts)
            .SingleAsync(payment => payment.Id == id);

        var winningAttempt = first.StartAttempt();
        stale.StartAttempt();
        await firstContext.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());

        await using var verificationScope = Factory.Services.CreateAsyncScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var payment = await context.Payments.Include(payment => payment.Attempts)
            .SingleAsync(payment => payment.Id == id);
        Assert.Equal(winningAttempt, Assert.Single(payment.Attempts).Id);
    }

    [Fact]
    public async Task Migration_preserves_an_existing_order_and_its_items()
    {
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync("20260101185054_InitialCreate");
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO Orders (Id, CustomerName, CreatedAt, Status)
                VALUES ({orderId}, {"Existing customer"}, {DateTime.UtcNow}, {(int)OrderStatus.Confirmed});
                INSERT INTO OrderItem (Id, ProductName, Quantity, UnitPriceAmount, UnitPriceCurrency, OrderId)
                VALUES ({itemId}, {"Existing product"}, {4}, {2.5m}, {"EUR"}, {orderId});
                """);
            await migrator.MigrateAsync();
        }

        var order = await ReadOrderAsync(orderId);
        Assert.Equal("Existing customer", order.CustomerName);
        Assert.Equal("Confirmed", order.Status);
        Assert.Equal(10m, order.TotalAmount);
        Assert.Single(order.Items);
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        var attempt = await SendAsync(new StartPaymentAttemptCommand(paymentId));
        await SendAsync(new SettlePaymentCommand(paymentId, attempt, 10, "EUR", "upgraded-order"));
        Assert.Equal("Paid", (await ReadOrderAsync(orderId)).Status);
    }
}
