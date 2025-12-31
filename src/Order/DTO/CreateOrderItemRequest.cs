namespace Order.DTO
{
    public sealed class CreateOrderItemRequest
    {
        public string? ProductName { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
