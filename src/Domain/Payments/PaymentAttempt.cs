namespace Commerce.Domain.Payments;

public sealed class PaymentAttempt
{
    public Guid Id { get; } = Guid.NewGuid();
    public PaymentAttemptStatus Status { get; private set; } = PaymentAttemptStatus.Pending;
    public string? ProviderTransactionReference { get; private set; }

    internal PaymentAttempt() { }

    internal void Fail()
    {
        if (Status == PaymentAttemptStatus.Failed)
            return;

        EnsurePending();
        Status = PaymentAttemptStatus.Failed;
    }

    internal void Succeed(string providerTransactionReference)
    {
        if (Status == PaymentAttemptStatus.Succeeded &&
            string.Equals(ProviderTransactionReference, providerTransactionReference, StringComparison.Ordinal))
            return;

        EnsurePending();
        ProviderTransactionReference = providerTransactionReference;
        Status = PaymentAttemptStatus.Succeeded;
    }

    private void EnsurePending()
    {
        if (Status != PaymentAttemptStatus.Pending)
            throw new InvalidOperationException("Only a pending payment attempt can be completed.");
    }
}
