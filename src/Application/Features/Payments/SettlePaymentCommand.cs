using Commerce.Application.Abstractions.Persistence;
using Commerce.Application.Common.Exceptions;
using Commerce.Domain.Common;
using Commerce.Domain.Payments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Features.Payments;

// Trusted application input. A future provider adapter must authenticate and verify
// the provider outcome before sending this command. It has no public HTTP endpoint.
public sealed record SettlePaymentCommand(
    Guid PaymentId,
    Guid AttemptId,
    decimal VerifiedAmount,
    string Currency,
    string ProviderTransactionReference) : IRequest;

public sealed class SettlePaymentCommandValidator : AbstractValidator<SettlePaymentCommand>
{
    public SettlePaymentCommandValidator()
    {
        RuleFor(command => command.PaymentId).NotEmpty();
        RuleFor(command => command.AttemptId).NotEmpty();
        RuleFor(command => command.VerifiedAmount).GreaterThan(0);
        RuleFor(command => command.Currency).NotEmpty().Matches("^[A-Za-z]{3}$");
        RuleFor(command => command.ProviderTransactionReference).NotEmpty().MaximumLength(200);
    }
}

public sealed class SettlePaymentCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    : IRequestHandler<SettlePaymentCommand>
{
    public async Task Handle(SettlePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await context.Payments.Include(payment => payment.Attempts)
            .SingleOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", request.PaymentId);
        var order = await context.Orders.Include(order => order.Items)
            .SingleAsync(order => order.Id == payment.OrderId, cancellationToken);

        var alreadyPaid = payment.IsPaid;
        var verifiedAmount = Money.Create(request.VerifiedAmount, request.Currency);
        payment.RecordSuccess(request.AttemptId, verifiedAmount, request.ProviderTransactionReference);
        order.MarkPaid(verifiedAmount);

        if (!alreadyPaid)
        {
            context.OrderPaidMessages.Add(new OrderPaidMessage
            {
                OrderId = order.Id.Value,
                PaymentId = payment.Id.Value,
                Amount = verifiedAmount.Amount,
                Currency = verifiedAmount.Currency,
                OccurredAtUtc = timeProvider.GetUtcNow()
            });
        }

        // One relational SaveChanges transaction commits payment, order and outbox.
        await context.SaveChangesAsync(cancellationToken);
    }
}
