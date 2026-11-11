using Commerce.Domain.Common;
using Commerce.Domain.Orders;
using Commerce.Domain.Payments;
using Xunit;

namespace Commerce.Domain.Tests.Payments;

public sealed class PaymentTests
{
    [Fact]
    public void FailedAttemptCanBeRetriedAndHistoryIsPreserved()
    {
        var orderId = OrderId.New();
        var amount = Money.Create(3, "EUR");
        var payment = Payment.Create(orderId, amount);
        var first = payment.StartAttempt();

        payment.RecordFailure(first);

        Assert.False(payment.IsPaid);
        var second = payment.StartAttempt();
        payment.RecordSuccess(second, amount, "test-transaction");

        Assert.Equal(orderId, payment.OrderId);
        Assert.True(payment.IsPaid);
        Assert.Collection(payment.Attempts,
            attempt => Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status),
            attempt => Assert.Equal(PaymentAttemptStatus.Succeeded, attempt.Status));
        Assert.Throws<InvalidOperationException>(() => payment.StartAttempt());
    }

    [Theory]
    [InlineData(2, "EUR")]
    [InlineData(3, "USD")]
    public void MismatchedAmountOrCurrencyCannotCompleteAnAttempt(int amount, string currency)
    {
        var payment = Payment.Create(OrderId.New(), Money.Create(3, "EUR"));
        var attempt = payment.StartAttempt();

        Assert.Throws<ArgumentException>(() =>
            payment.RecordSuccess(attempt, Money.Create(amount, currency), "test-transaction"));

        Assert.False(payment.IsPaid);
        Assert.Equal(PaymentAttemptStatus.Pending, Assert.Single(payment.Attempts).Status);
    }

    [Fact]
    public void ReplayedSuccessIsIdempotentButConflictingResultsAreRejected()
    {
        var amount = Money.Create(3, "EUR");
        var payment = Payment.Create(OrderId.New(), amount);
        var attempt = payment.StartAttempt();

        Assert.Throws<InvalidOperationException>(() => payment.StartAttempt());
        payment.RecordSuccess(attempt, amount, "test-transaction");
        payment.RecordSuccess(attempt, amount, "test-transaction");

        Assert.Equal("test-transaction", Assert.Single(payment.Attempts).ProviderTransactionReference);
        Assert.Throws<InvalidOperationException>(() =>
            payment.RecordSuccess(attempt, amount, "conflicting-transaction"));
        Assert.Throws<InvalidOperationException>(() => payment.RecordFailure(attempt));
    }
}
