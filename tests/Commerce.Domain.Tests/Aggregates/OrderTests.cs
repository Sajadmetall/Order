using Commerce.Domain.Common;
using Commerce.Domain.Orders;
using Commerce.Domain.Tests.Builders;
using FluentAssertions;
using Xunit;

namespace Commerce.Domain.Tests.Aggregates;

public class OrderTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 10, 12, 30, 0, TimeSpan.FromHours(1));
    [Fact]
    public void Create_WithValidCustomerName_ShouldCreateOrder()
    {
        // Arrange
        var customerName = "John Doe";

        // Act
        var order = Order.Create(customerName, CreatedAt);

        // Assert
        order.Should().NotBeNull();
        order.CustomerName.Should().Be(customerName);
        order.Status.Should().Be(OrderStatus.Created);
        order.Items.Should().BeEmpty();

        order.Id.Should().NotBeNull();
        order.CreatedAt.Should().Be(CreatedAt.UtcDateTime);
        order.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCustomerName_ShouldThrowArgumentException(string customerName)
    {
        // Act
        var act = () => Order.Create(customerName, CreatedAt);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("customerName");
    }

    [Fact]
    public void AddItem_WhenOrderIsCreated_ShouldAddItem()
    {
        // Arrange
        var order = OrderBuilder.Empty().Build();
        var price = Money.Create(10m, "EUR");

        // Act
        order.AddItem("Product A", 2, price);

        // Assert
        order.Items.Should().HaveCount(1);
        var item = order.Items.Single();
        item.ProductName.Should().Be("Product A");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(price);
    }

    [Fact]
    public void AddItem_WhenOrderIsNotCreated_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        order.Confirm(); // now status != Created

        // Act
        var act = () => order.AddItem("Product B", 1, Money.Create(5m, "EUR"));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Created state*");
    }

    [Fact]
    public void Confirm_WhenOrderHasNoItems_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var order = OrderBuilder.Empty().Build();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*at least one item*");
    }

    [Fact]
    public void Confirm_WhenOrderHasItems_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenOrderIsNotCreated_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        order.Confirm(); // now Confirmed

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Created state*");
    }

    [Fact]
    public void Ship_WhenOrderIsPaid_ShouldChangeStatusToShipped()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        order.Confirm();

        order.MarkPaid(order.GetTotal());

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Theory]
    [InlineData(OrderStatus.Created)]
    [InlineData(OrderStatus.Confirmed)]
    public void Ship_WhenOrderIsUnpaid_ShouldThrowBusinessRuleViolationException(OrderStatus status)
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        if (status == OrderStatus.Confirmed)
            order.Confirm();

        // Act
        var act = () => order.Ship();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*paid order*");
        order.Status.Should().Be(status);
    }
}
