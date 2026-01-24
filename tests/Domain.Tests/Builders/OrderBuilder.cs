using Domain.Aggregates;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Builders;

/// <summary>
/// Builder for creating Order aggregate instances in tests using the Builder pattern.
/// Provides a fluent API for test data setup.
/// </summary>
public class OrderBuilder
{
    private OrderId _id = TestIds.DefaultOrderId;
    private string _customerName = "Test Customer";
    private DateTime _createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
    private readonly List<OrderItem> _items = [];

    /// <summary>
    /// Sets the order ID.
    /// </summary>
    public OrderBuilder WithId(OrderId id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the order ID using a Guid.
    /// </summary>
    public OrderBuilder WithId(Guid id)
    {
        _id = OrderId.Create(id);
        return this;
    }

    /// <summary>
    /// Sets the customer name.
    /// </summary>
    public OrderBuilder WithCustomerName(string customerName)
    {
        _customerName = customerName;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    public OrderBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Adds a single order item.
    /// </summary>
    public OrderBuilder AddItem(string productName, int quantity, decimal unitPrice, string currency = "USD")
    {
        var money = Money.Create(unitPrice, currency);
        var item = OrderItem.Create(productName, quantity, money);
        _items.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a pre-constructed order item.
    /// </summary>
    public OrderBuilder AddItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a default item for quick test setup (Laptop, qty: 1, price: $999.99 USD).
    /// </summary>
    public OrderBuilder WithDefaultItem()
    {
        return AddItem("Laptop", 1, 999.99m, "USD");
    }

    /// <summary>
    /// Adds multiple standard items for testing collections.
    /// </summary>
    public OrderBuilder WithMultipleItems()
    {
        return AddItem("Laptop", 2, 999.99m, "USD")
               .AddItem("Mouse", 3, 25.50m, "USD")
               .AddItem("Keyboard", 1, 89.99m, "USD");
    }

    /// <summary>
    /// Adds items with different currencies for testing multi-currency scenarios.
    /// </summary>
    public OrderBuilder WithMixedCurrencyItems()
    {
        return AddItem("Laptop", 1, 999.99m, "USD")
               .AddItem("Monitor", 1, 450.00m, "EUR")
               .AddItem("Headphones", 1, 12000m, "JPY");
    }

    /// <summary>
    /// Creates an order without any items.
    /// </summary>
    public OrderBuilder WithoutItems()
    {
        _items.Clear();
        return this;
    }

    /// <summary>
    /// Builds the Order aggregate instance.
    /// </summary>
    public Order Build()
    {
        var order = Order.Create(_id, _customerName, _createdAt);
        
        foreach (var item in _items)
        {
            order.AddItem(item);
        }

        return order;
    }

    /// <summary>
    /// Creates a builder with default configuration (includes one default item).
    /// </summary>
    public static OrderBuilder Default() => new OrderBuilder().WithDefaultItem();

    /// <summary>
    /// Creates a builder for an empty order (no items).
    /// </summary>
    public static OrderBuilder Empty() => new OrderBuilder().WithoutItems();
}using Domain.Aggregates;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Builders;

/// <summary>
/// Builder for creating Order aggregate instances in tests using the Builder pattern.
/// Provides a fluent API for test data setup.
/// </summary>
public class OrderBuilder
{
    private OrderId _id = TestIds.DefaultOrderId;
    private string _customerName = "Test Customer";
    private DateTime _createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
    private readonly List<OrderItem> _items = [];

    /// <summary>
    /// Sets the order ID.
    /// </summary>
    public OrderBuilder WithId(OrderId id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the order ID using a Guid.
    /// </summary>
    public OrderBuilder WithId(Guid id)
    {
        _id = OrderId.Create(id);
        return this;
    }

    /// <summary>
    /// Sets the customer name.
    /// </summary>
    public OrderBuilder WithCustomerName(string customerName)
    {
        _customerName = customerName;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    public OrderBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Adds a single order item.
    /// </summary>
    public OrderBuilder AddItem(string productName, int quantity, decimal unitPrice, string currency = "USD")
    {
        var money = Money.Create(unitPrice, currency);
        var item = OrderItem.Create(productName, quantity, money);
        _items.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a pre-constructed order item.
    /// </summary>
    public OrderBuilder AddItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a default item for quick test setup (Laptop, qty: 1, price: $999.99 USD).
    /// </summary>
    public OrderBuilder WithDefaultItem()
    {
        return AddItem("Laptop", 1, 999.99m, "USD");
    }

    /// <summary>
    /// Adds multiple standard items for testing collections.
    /// </summary>
    public OrderBuilder WithMultipleItems()
    {
        return AddItem("Laptop", 2, 999.99m, "USD")
               .AddItem("Mouse", 3, 25.50m, "USD")
               .AddItem("Keyboard", 1, 89.99m, "USD");
    }

    /// <summary>
    /// Adds items with different currencies for testing multi-currency scenarios.
    /// </summary>
    public OrderBuilder WithMixedCurrencyItems()
    {
        return AddItem("Laptop", 1, 999.99m, "USD")
               .AddItem("Monitor", 1, 450.00m, "EUR")
               .AddItem("Headphones", 1, 12000m, "JPY");
    }

    /// <summary>
    /// Creates an order without any items.
    /// </summary>
    public OrderBuilder WithoutItems()
    {
        _items.Clear();
        return this;
    }

    /// <summary>
    /// Builds the Order aggregate instance.
    /// </summary>
    public Order Build()
    {
        var order = Order.Create(_id, _customerName, _createdAt);
        
        foreach (var item in _items)
        {
            order.AddItem(item);
        }

        return order;
    }

    /// <summary>
    /// Creates a builder with default configuration (includes one default item).
    /// </summary>
    public static OrderBuilder Default() => new OrderBuilder().WithDefaultItem();

    /// <summary>
    /// Creates a builder for an empty order (no items).
    /// </summary>
    public static OrderBuilder Empty() => new OrderBuilder().WithoutItems();
}using Domain.Aggregates;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Builders;

/// <summary>
/// Builder for creating Order aggregate instances in tests using the Builder pattern.
/// Provides a fluent API for test data setup.
/// </summary>
public class OrderBuilder
{
    private OrderId _id = TestIds.DefaultOrderId;
    private string _customerName = "Test Customer";
    private DateTime _createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
    private readonly List<OrderItem> _items = [];

    /// <summary>
    /// Sets the order ID.
    /// </summary>
    public OrderBuilder WithId(OrderId id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the order ID using a Guid.
    /// </summary>
    public OrderBuilder WithId(Guid id)
    {
        _id = OrderId.Create(id);
        return this;
    }

    /// <summary>
    /// Sets the customer name.
    /// </summary>
    public OrderBuilder WithCustomerName(string customerName)
    {
        _customerName = customerName;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    public OrderBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Adds a single order item.
    /// </summary>
    public OrderBuilder AddItem(string productName, int quantity, decimal unitPrice, string currency = "USD")
    {
        var money = Money.Create(unitPrice, currency);
        var item = OrderItem.Create(productName, quantity, money);
        _items.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a pre-constructed order item.
    /// </summary>
    public OrderBuilder AddItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a default item for quick test setup (Laptop, qty: 1, price: $999.99 USD).
    /// </summary>
    public OrderBuilder WithDefaultItem()
    {
        return AddItem("Laptop", 1, 999.99m, "USD");
    }

    /// <summary>
    /// Adds multiple standard items for testing collections.
    /// </summary>
    public OrderBuilder WithMultipleItems()
    {
        return AddItem("Laptop", 2, 999.99m, "USD")
               .AddItem("Mouse", 3, 25.50m, "USD")
               .AddItem("Keyboard", 1, 89.99m, "USD");
    }

    /// <summary>
    /// Adds items with different currencies for testing multi-currency scenarios.
    /// </summary>
    public OrderBuilder WithMixedCurrencyItems()
    {
        return AddItem("Laptop", 1, 999.99m, "USD")
               .AddItem("Monitor", 1, 450.00m, "EUR")
               .AddItem("Headphones", 1, 12000m, "JPY");
    }

    /// <summary>
    /// Creates an order without any items.
    /// </summary>
    public OrderBuilder WithoutItems()
    {
        _items.Clear();
        return this;
    }

    /// <summary>
    /// Builds the Order aggregate instance.
    /// </summary>
    public Order Build()
    {
        var order = Order.Create(_id, _customerName, _createdAt);
        
        foreach (var item in _items)
        {
            order.AddItem(item);
        }

        return order;
    }

    /// <summary>
    /// Creates a builder with default configuration (includes one default item).
    /// </summary>
    public static OrderBuilder Default() => new OrderBuilder().WithDefaultItem();

    /// <summary>
    /// Creates a builder for an empty order (no items).
    /// </summary>
    public static OrderBuilder Empty() => new OrderBuilder().WithoutItems();
}