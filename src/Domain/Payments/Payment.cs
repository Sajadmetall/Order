using Commerce.Domain.Common;
using Commerce.Domain.Orders;

namespace Commerce.Domain.Payments;

public sealed class Payment
{
    private readonly List<PaymentAttempt> _attempts = [];

    public PaymentId Id { get; }
    public OrderId OrderId { get; }
    public Money Amount { get; }
    public IReadOnlyList<PaymentAttempt> Attempts => _attempts.AsReadOnly();
    public bool IsPaid => _attempts.Any(attempt => attempt.Status == PaymentAttemptStatus.Succeeded);

    private Payment(OrderId orderId, Money amount)
    {
        Id = PaymentId.New();
        OrderId = orderId;
        Amount = amount;
    }

    public static Payment Create(OrderId orderId, Money amount)
    {
        ArgumentNullException.ThrowIfNull(orderId);
        ArgumentNullException.ThrowIfNull(amount);
        if (orderId.Value == Guid.Empty)
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        if (amount.Amount <= 0 || string.IsNullOrWhiteSpace(amount.Currency))
            throw new ArgumentException("Payment requires a positive amount and a currency.", nameof(amount));

        return new Payment(orderId, amount);
    }

    public Guid StartAttempt()
    {
        if (IsPaid)
            throw new InvalidOperationException("A paid payment cannot start another attempt.");
        if (_attempts.Any(attempt => attempt.Status == PaymentAttemptStatus.Pending))
            throw new InvalidOperationException("A payment attempt is already pending.");

        var attempt = new PaymentAttempt();
        _attempts.Add(attempt);
        return attempt.Id;
    }

    public void RecordFailure(Guid attemptId) => FindAttempt(attemptId).Fail();

    public void RecordSuccess(Guid attemptId, Money verifiedAmount, string providerTransactionReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerTransactionReference);
        if (verifiedAmount != Amount)
            throw new ArgumentException("Verified payment must match the expected amount and currency.", nameof(verifiedAmount));

        FindAttempt(attemptId).Succeed(providerTransactionReference);
    }

    private PaymentAttempt FindAttempt(Guid attemptId) =>
        _attempts.SingleOrDefault(attempt => attempt.Id == attemptId)
        ?? throw new ArgumentException("The attempt does not belong to this payment.", nameof(attemptId));
}
