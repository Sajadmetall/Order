namespace Order.DTO
{
    public sealed record CreateOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency);
}
