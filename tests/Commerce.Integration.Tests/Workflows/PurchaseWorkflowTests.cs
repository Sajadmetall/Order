using Commerce.Application.Common.Exceptions;
using MassTransit.EntityFrameworkCoreIntegration;
using System.Text.Json;
using Commerce.Application.Features.Payments;
using Commerce.Domain.Payments;
using Commerce.Infrastructure.Persistence;
using Commerce.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Commerce.Integration.Tests.Workflows;

[Trait("Category", "Integration")]
public sealed class PurchaseWorkflowTests(CommerceFixture infrastructure)
    : PurchaseTestBase(infrastructure), IClassFixture<CommerceFixture>
{
    [Fact]
    public async Task Failed_attempt_then_retry_settles_the_order_and_records_one_purchase_outcome()
    {
        var orderId = await CreateOrderAsync();
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        var firstAttempt = await SendAsync(new StartPaymentAttemptCommand(paymentId));

        await SendAsync(new RecordPaymentFailureCommand(paymentId, firstAttempt));
        Assert.Equal("Confirmed", (await ReadOrderAsync(orderId)).Status);
        await using (var failedScope = Factory.Services.CreateAsyncScope())
        {
            var context = failedScope.ServiceProvider.GetRequiredService<OrderDbContext>();
            Assert.Empty(await context.Set<OutboxMessage>().ToListAsync());
        }

        var secondAttempt = await SendAsync(new StartPaymentAttemptCommand(paymentId));
        var transaction = "integration-transaction-" + Guid.NewGuid().ToString("N");
        await SendAsync(new SettlePaymentCommand(paymentId, secondAttempt, 10, "EUR", transaction));

        var order = await ReadOrderAsync(orderId);
        Assert.Equal("Paid", order.Status);
        Assert.Equal(10m, order.TotalAmount);
        Assert.Equal("EUR", order.Currency);

        // A fresh scope proves that the outcome and the full attempt history were persisted.
        await using var scope = Factory.Services.CreateAsyncScope();
        var saved = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var payment = await saved.Payments.Include(payment => payment.Attempts).SingleAsync();
        Assert.Equal(orderId, payment.OrderId.Value);
        Assert.True(payment.IsPaid);
        Assert.Equal(2, payment.Attempts.Count);
        Assert.Equal(PaymentAttemptStatus.Failed, payment.Attempts.Single(attempt => attempt.Id == firstAttempt).Status);
        Assert.Equal(PaymentAttemptStatus.Succeeded, payment.Attempts.Single(attempt => attempt.Id == secondAttempt).Status);
        var outcome = DeserializeOutcome(await saved.Set<OutboxMessage>().SingleAsync());
        Assert.Equal(orderId, outcome.OrderId);
        Assert.Equal(paymentId, outcome.PaymentId);
        Assert.Equal(10m, outcome.Amount);
        Assert.Equal("EUR", outcome.Currency);
    }

    [Fact]
    public async Task Repeating_payment_preparation_reuses_the_same_payment()
    {
        var orderId = await CreateOrderAsync();
        var first = await SendAsync(new PreparePaymentCommand(orderId));
        var repeated = await SendAsync(new PreparePaymentCommand(orderId));

        Assert.Equal(first, repeated);
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        Assert.Equal(1, await context.Payments.CountAsync());
    }

    [Theory]
    [InlineData(9, "EUR")]
    [InlineData(10, "USD")]
    public async Task Mismatched_result_keeps_order_and_attempt_unsettled(decimal amount, string currency)
    {
        var orderId = await CreateOrderAsync();
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        var attempt = await SendAsync(new StartPaymentAttemptCommand(paymentId));

        await Assert.ThrowsAsync<ArgumentException>(() => SendAsync(
            new SettlePaymentCommand(paymentId, attempt, amount, currency, "mismatch")));

        Assert.Equal("Confirmed", (await ReadOrderAsync(orderId)).Status);
        await AssertPendingPaymentAsync(paymentId);
    }

    [Fact]
    public async Task Replayed_success_in_another_scope_creates_no_duplicate_purchase_outcome()
    {
        var orderId = await CreateOrderAsync();
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        var attempt = await SendAsync(new StartPaymentAttemptCommand(paymentId));
        var result = new SettlePaymentCommand(paymentId, attempt, 10, "EUR", "replayed-result");

        await SendAsync(result);
        await SendAsync(result);

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        Assert.Equal(1, await context.Set<OutboxMessage>().CountAsync());
        Assert.Single((await context.Payments.Include(payment => payment.Attempts).SingleAsync()).Attempts);
        Assert.Equal("Paid", (await ReadOrderAsync(orderId)).Status);
    }

    [Fact]
    public async Task Conflicting_replay_does_not_replace_the_original_transaction()
    {
        var orderId = await CreateOrderAsync();
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        var attempt = await SendAsync(new StartPaymentAttemptCommand(paymentId));
        await SendAsync(new SettlePaymentCommand(paymentId, attempt, 10, "EUR", "original-result"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync(
            new SettlePaymentCommand(paymentId, attempt, 10, "EUR", "different-result")));

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var payment = await context.Payments.Include(payment => payment.Attempts).SingleAsync();
        Assert.Equal("original-result", Assert.Single(payment.Attempts).ProviderTransactionReference);
        Assert.Equal(1, await context.Set<OutboxMessage>().CountAsync());
    }

    [Fact]
    public async Task Database_rejecting_reused_provider_transaction_rolls_back_the_entire_settlement()
    {
        var firstOrder = await CreateOrderAsync();
        var firstPayment = await SendAsync(new PreparePaymentCommand(firstOrder));
        var firstAttempt = await SendAsync(new StartPaymentAttemptCommand(firstPayment));
        await SendAsync(new SettlePaymentCommand(firstPayment, firstAttempt, 10, "EUR", "unique-result"));
        var secondOrder = await CreateOrderAsync();
        var secondPayment = await SendAsync(new PreparePaymentCommand(secondOrder));
        var secondAttempt = await SendAsync(new StartPaymentAttemptCommand(secondPayment));

        await Assert.ThrowsAsync<DbUpdateException>(() => SendAsync(
            new SettlePaymentCommand(secondPayment, secondAttempt, 10, "EUR", "unique-result")));

        Assert.Equal("Paid", (await ReadOrderAsync(firstOrder)).Status);
        Assert.Equal("Confirmed", (await ReadOrderAsync(secondOrder)).Status);
        await AssertPendingPaymentAsync(secondPayment);
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        Assert.Equal(firstOrder, DeserializeOutcome(await context.Set<OutboxMessage>().SingleAsync()).OrderId);
    }

    [Fact]
    public async Task A_second_pending_attempt_is_rejected_after_reloading_payment()
    {
        var orderId = await CreateOrderAsync();
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        await SendAsync(new StartPaymentAttemptCommand(paymentId));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SendAsync(new StartPaymentAttemptCommand(paymentId)));
        await AssertPendingPaymentAsync(paymentId);
    }

    [Fact]
    public async Task An_attempt_from_another_payment_cannot_settle_the_order()
    {
        var firstOrder = await CreateOrderAsync();
        var firstPayment = await SendAsync(new PreparePaymentCommand(firstOrder));
        var firstAttempt = await SendAsync(new StartPaymentAttemptCommand(firstPayment));
        var secondOrder = await CreateOrderAsync();
        var secondPayment = await SendAsync(new PreparePaymentCommand(secondOrder));
        await SendAsync(new StartPaymentAttemptCommand(secondPayment));

        await Assert.ThrowsAsync<ArgumentException>(() => SendAsync(
            new SettlePaymentCommand(secondPayment, firstAttempt, 10, "EUR", "wrong-attempt")));

        Assert.Equal("Confirmed", (await ReadOrderAsync(firstOrder)).Status);
        Assert.Equal("Confirmed", (await ReadOrderAsync(secondOrder)).Status);
        await AssertPendingPaymentAsync(secondPayment);
    }

    [Fact]
    public async Task Unknown_order_does_not_create_payment()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            SendAsync(new PreparePaymentCommand(Guid.NewGuid())));

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        Assert.Empty(await context.Payments.ToListAsync());
    }

    private static OrderPaidMessage DeserializeOutcome(OutboxMessage row)
    {
        using var envelope = JsonDocument.Parse(row.Body);
        return envelope.RootElement.GetProperty("message").Deserialize<OrderPaidMessage>(
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    private async Task AssertPendingPaymentAsync(Guid paymentId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var id = new PaymentId(paymentId);
        var payment = await context.Payments.Include(payment => payment.Attempts)
            .SingleAsync(payment => payment.Id == id);
        Assert.False(payment.IsPaid);
        Assert.Equal(PaymentAttemptStatus.Pending, Assert.Single(payment.Attempts).Status);
        var outcomes = await context.Set<OutboxMessage>().ToListAsync();
        Assert.DoesNotContain(outcomes, message => DeserializeOutcome(message).PaymentId == paymentId);
    }
}
