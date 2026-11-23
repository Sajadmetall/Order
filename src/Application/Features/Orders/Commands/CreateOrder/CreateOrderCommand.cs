using MediatR;

namespace Commerce.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    string CustomerName,
    List<CreateOrderItemDto> Items
) : IRequest<Guid>;

public sealed record CreateOrderItemDto(
    string ProductName,
    int Quantity,
    decimal UnitPriceAmount,
    string UnitPriceCurrency
);