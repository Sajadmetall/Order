using Commerce.Application.Abstractions.Persistence;
using Commerce.Application.Common.Exceptions;
using Commerce.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Features.Orders.Commands;

public sealed record ConfirmOrderCommand(Guid OrderId) : IRequest;

public sealed class ConfirmOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ConfirmOrderCommand>
{
    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var id = new OrderId(request.OrderId);
        var order = await context.Orders.Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        order.Confirm();
        await context.SaveChangesAsync(cancellationToken);
    }
}
