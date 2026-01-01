namespace Order.DTO
{
    
    public sealed record OrderItemResponse(
    Guid Id,
    string ProductName,
    int Quantity,
    decimal UnitPriceAmount,
    decimal TotalPriceAmount);
}
