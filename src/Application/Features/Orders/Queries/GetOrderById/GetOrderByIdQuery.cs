using Commerce.Application.Features.Orders.Dtos;
using MediatR;

namespace Commerce.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
