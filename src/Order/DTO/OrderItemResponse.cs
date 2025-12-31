namespace Order.DTO
{
    public sealed class OrderItemResponse
    {
        public string ProductName { get; init; } = default!;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
