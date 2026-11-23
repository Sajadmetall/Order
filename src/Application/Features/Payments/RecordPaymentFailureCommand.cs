using Commerce.Application.Abstractions.Persistence;
using Commerce.Application.Common.Exceptions;
using Commerce.Domain.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Features.Payments;

public sealed record RecordPaymentFailureCommand(Guid PaymentId, Guid AttemptId) : IRequest;

public sealed class RecordPaymentFailureCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RecordPaymentFailureCommand>
{
    public async Task Handle(RecordPaymentFailureCommand request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await context.Payments.Include(payment => payment.Attempts)
            .SingleOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", request.PaymentId);

        payment.RecordFailure(request.AttemptId);
        await context.SaveChangesAsync(cancellationToken);
    }
}
