using Domain.Orders;

namespace Order.DTO
{
    public sealed class OrderResponse
    {
        public Guid Id { get; init; }
        public string CustomerName { get; init; } = default!;
        public DateTime CreatedAt { get; init; }
        public OrderStatus Status { get; init; }
        public List<OrderItemResponse> Items { get; init; } = new();
    }
}
