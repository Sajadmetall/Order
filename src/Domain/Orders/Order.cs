using Commerce.Domain.Common;
namespace Commerce.Domain.Orders;

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

    private Order(string customerName, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("CustomerName is required.", nameof(customerName));

        Id = OrderId.New();
        CustomerName = customerName;
        CreatedAt = createdAt.UtcDateTime;
        Status = OrderStatus.Created;
    }

    public static Order Create(string customerName, DateTimeOffset createdAt)
        => new(customerName, createdAt);

    public void AddItem(string productName, int quantity, Money unitPrice)
    {
        EnsureIsCreated();

        if (_items.Count > 0 && _items[0].UnitPrice.Currency != unitPrice.Currency)
            throw new BusinessRuleViolationException("All order items must use the same currency.");

        // Order (root) creates internal entity
        _items.Add(new OrderItem(productName, quantity, unitPrice));
    }

    public void Confirm()
    {
        EnsureIsCreated();
        if (_items.Count == 0)
            throw new BusinessRuleViolationException("Order must have at least one item.");
        Status = OrderStatus.Confirmed;
    }

    public Money GetTotal()
    {
        if (_items.Count == 0)
            throw new BusinessRuleViolationException("An empty order has no payable total.");

        return _items.Select(item => item.TotalPrice).Aggregate((total, amount) => total.Add(amount));
    }

    public void MarkPaid(Money verifiedAmount)
    {
        if (verifiedAmount != GetTotal())
            throw new ArgumentException("Payment must match the order total and currency.", nameof(verifiedAmount));
        if (Status == OrderStatus.Paid)
            return;
        if (Status != OrderStatus.Confirmed)
            throw new BusinessRuleViolationException("Only a confirmed order can be paid.");

        Status = OrderStatus.Paid;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new BusinessRuleViolationException("Only a paid order can be shipped.");
        Status = OrderStatus.Shipped;
    }

    private void EnsureIsCreated()
    {
        if (Status != OrderStatus.Created)
            throw new BusinessRuleViolationException("Order can only be changed in Created state.");
    }
}
