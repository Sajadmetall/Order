namespace Domain.Orders;

public sealed class OrderItem
{
    public Guid Id { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public Money UnitPrice { get; }

    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    internal OrderItem(string productName, int quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        Id = Guid.NewGuid();
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}