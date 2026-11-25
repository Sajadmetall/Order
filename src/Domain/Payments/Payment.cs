using Commerce.Domain.Common;
using Commerce.Domain.Orders;

namespace Commerce.Domain.Payments;

public sealed class Payment
{
    private readonly List<PaymentAttempt> _attempts = [];

    public PaymentId Id { get; private set; }
    public OrderId OrderId { get; private set; } = default!;
    public Money Amount { get; private set; } = default!;
    public int Revision { get; private set; }
    public IReadOnlyList<PaymentAttempt> Attempts => _attempts.AsReadOnly();
    public bool IsPaid => _attempts.Any(attempt => attempt.Status == PaymentAttemptStatus.Succeeded);

    private Payment() { } // EF Core materialization

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
            throw new BusinessRuleViolationException("A paid payment cannot start another attempt.");
        if (_attempts.Any(attempt => attempt.Status == PaymentAttemptStatus.Pending))
            throw new BusinessRuleViolationException("A payment attempt is already pending.");

        var attempt = new PaymentAttempt();
        _attempts.Add(attempt);
        Revision++;
        return attempt.Id;
    }

    public void RecordFailure(Guid attemptId)
    {
        var attempt = FindAttempt(attemptId);
        var previousStatus = attempt.Status;
        attempt.Fail();
        if (previousStatus != attempt.Status)
            Revision++;
    }

    public void RecordSuccess(Guid attemptId, Money verifiedAmount, string providerTransactionReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerTransactionReference);
        if (verifiedAmount != Amount)
            throw new ArgumentException("Verified payment must match the expected amount and currency.", nameof(verifiedAmount));

        var attempt = FindAttempt(attemptId);
        var previousStatus = attempt.Status;
        attempt.Succeed(providerTransactionReference);
        if (previousStatus != attempt.Status)
            Revision++;
    }

    private PaymentAttempt FindAttempt(Guid attemptId) =>
        _attempts.SingleOrDefault(attempt => attempt.Id == attemptId)
        ?? throw new ArgumentException("The attempt does not belong to this payment.", nameof(attemptId));
}
