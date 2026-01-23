using Application.Abstractions.Persistence;
using Application.Common.Exceptions;
using Application.Features.Orders.Dtos;
using Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var orderDto = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == new OrderId(request.OrderId))
            .Select(o => new OrderDto
            {
                Id = o.Id.Value,
                CustomerName = o.CustomerName,
                CreatedAt = o.CreatedAt,
                Status = o.Status.ToString(),

                TotalAmount = o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity),
                Currency = o.Items.Select(i => i.UnitPrice.Currency).FirstOrDefault() ?? string.Empty,

                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPriceAmount = i.UnitPrice.Amount,
                    SubtotalAmount = i.UnitPrice.Amount * i.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (orderDto is null)
            throw new NotFoundException("Order", request.OrderId);

        return orderDto;
    }
}
