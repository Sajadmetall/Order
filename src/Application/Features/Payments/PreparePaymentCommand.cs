using Commerce.Application.Abstractions.Persistence;
using Commerce.Application.Common.Exceptions;
using Commerce.Domain.Orders;
using Commerce.Domain.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Features.Payments;

public sealed record PreparePaymentCommand(Guid OrderId) : IRequest<Guid>;

public sealed class PreparePaymentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<PreparePaymentCommand, Guid>
{
    public async Task<Guid> Handle(PreparePaymentCommand request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await context.Orders.Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        var existing = await context.Payments
            .SingleOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);
        if (existing is not null)
            return existing.Id.Value;

        if (order.Status == OrderStatus.Created)
            order.Confirm();
        if (order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only a confirmed order can start payment.");

        // Derive the expected amount from persisted items, including quantity.
        var payment = Payment.Create(order.Id, order.GetTotal());
        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
        return payment.Id.Value;
    }
}
