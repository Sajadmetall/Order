namespace Commerce.Api.DTO
{
    public sealed record CreateOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency);
}
