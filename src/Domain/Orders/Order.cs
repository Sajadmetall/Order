namespace Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    public OrderId Id { get; }
    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money TotalPrice =>
        _items.Aggregate(Money.Zero("EUR"), (sum, item) => sum.Add(item.TotalPrice));

    private Order(OrderId id)
    {
        Id = id;
        Status = OrderStatus.Created;
    }

    public static Order CreateNew()
    {
        return new Order(OrderId.New());
    }

    public void AddItem(string productName, int quantity, Money unitPrice)
    {
        EnsureIsModifiable();

        _items.Add(new OrderItem(productName, quantity, unitPrice));
    }

    public void Submit()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Order must have at least one item.");

        Status = OrderStatus.Submitted;
    }

    private void EnsureIsModifiable()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException("Order cannot be modified in current state.");
    }
}
