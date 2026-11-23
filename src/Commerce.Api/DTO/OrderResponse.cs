using Commerce.Domain.Orders;

namespace Commerce.Api.DTO
{
    public sealed record OrderResponse(
    Guid Id,
    string CustomerName,
    DateTime CreatedAt,
    string Status,
    decimal TotalAmount,
    string Currency,
    List<OrderItemResponse> Items);
}   
