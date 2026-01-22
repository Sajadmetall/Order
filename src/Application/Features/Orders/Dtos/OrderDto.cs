namespace Application.Features.Orders.Dtos;

public sealed class OrderDto
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string Status { get; init; } = string.Empty;

    // Read-side optimized
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;

    public List<OrderItemDto> Items { get; init; } = new();
}
public sealed class OrderItemDto
{
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }

    public decimal UnitPriceAmount { get; init; }
    public decimal SubtotalAmount { get; init; }
}