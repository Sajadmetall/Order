using Application.Abstractions.Persistence;
using Domain.Orders;
using MediatR;

namespace Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1) Create aggregate root using domain factory
        var order = Order.Create(request.CustomerName);

        // 2) Add items using domain method (enforces invariants)
        foreach (var item in request.Items)
        {
            var unitPrice =  Money.Create(item.UnitPriceAmount, item.UnitPriceCurrency);
            order.AddItem(item.ProductName, item.Quantity, unitPrice);
        }

        // 3) Persist
        _context.Orders.Add(order);

        // 4) Commit (EF DbContext is the UoW in this approach)
        await _context.SaveChangesAsync(cancellationToken);

        // 5) Return id (assuming OrderId wraps a Guid in a Value property)
        return order.Id.Value;
    }
}
