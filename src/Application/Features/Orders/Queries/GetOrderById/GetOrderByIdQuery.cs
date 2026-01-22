using Application.Features.Orders.Dtos;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
