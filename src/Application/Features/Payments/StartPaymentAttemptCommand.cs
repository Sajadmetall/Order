using Commerce.Application.Abstractions.Persistence;
using Commerce.Application.Common.Exceptions;
using Commerce.Domain.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Features.Payments;

public sealed record StartPaymentAttemptCommand(Guid PaymentId) : IRequest<Guid>;

public sealed class StartPaymentAttemptCommandHandler(IApplicationDbContext context)
    : IRequestHandler<StartPaymentAttemptCommand, Guid>
{
    public async Task<Guid> Handle(StartPaymentAttemptCommand request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await context.Payments.Include(payment => payment.Attempts)
            .SingleOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", request.PaymentId);

        var attemptId = payment.StartAttempt();
        await context.SaveChangesAsync(cancellationToken);
        return attemptId;
    }
}
