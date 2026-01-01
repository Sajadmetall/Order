namespace Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    public OrderId Id { get; private set; }
    public string CustomerName { get; private set; } = default!;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }

    private Order()
    {
        // For EF
    }

    private Order(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("CustomerName is required.", nameof(customerName));

        Id = OrderId.New();
        CustomerName = customerName;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Created;
    }

    public static Order Create(string customerName)
        => new(customerName);

    public void AddItem(string productName, int quantity, Money unitPrice)
    {
        EnsureIsCreated();

        // Order (root) creates internal entity
        _items.Add(new OrderItem(productName, quantity, unitPrice));
    }

    public void Confirm()
    {
        EnsureIsCreated();
        if (_items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item.");
        Status = OrderStatus.Confirmed;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Order can only be shipped when in Confirmed state.");
        Status = OrderStatus.Shipped;
    }

    private void EnsureIsCreated()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException("Order can only be changed in Created state.");
    }
}
