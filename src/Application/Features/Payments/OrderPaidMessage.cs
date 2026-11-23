namespace Commerce.Application.Features.Payments;

// A durable purchase outcome, ready for a future outbox dispatcher.
public sealed class OrderPaidMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; init; }
}
