using Commerce.Application.Abstractions.Persistence;
using Commerce.Application.Common.Exceptions;
using Commerce.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Application.Features.Orders.Commands;

public sealed record ShipOrderCommand(Guid OrderId) : IRequest;

public sealed class ShipOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ShipOrderCommand>
{
    public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var id = new OrderId(request.OrderId);
        var order = await context.Orders.SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        order.Ship();
        await context.SaveChangesAsync(cancellationToken);
    }
}
