using Domain.Orders;

namespace Domain.Tests.Builders;

public sealed class OrderBuilder
{
    private string _customerName = "John Doe";
    private readonly List<(string name, int qty, Money price)> _items = new();

    public static OrderBuilder Empty() => new();

    public static OrderBuilder WithOneItem()
        => new OrderBuilder().WithItem("Product", 1, Money.Create(10m, "EUR"));

    public OrderBuilder WithCustomer(string customerName)
    {
        _customerName = customerName;
        return this;
    }

    public OrderBuilder WithItem(string productName, int quantity, Money unitPrice)
    {
        _items.Add((productName, quantity, unitPrice));
        return this;
    }

    public Order Build()
    {
        var order = Order.Create(_customerName);

        foreach (var (name, qty, price) in _items)
        {
            order.AddItem(name, qty, price);
        }

        return order;
    }
}
