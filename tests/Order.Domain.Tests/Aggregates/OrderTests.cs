using Domain.Orders;
using Domain.Tests.Builders;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.Aggregates;

public class OrderTests
{
    [Fact]
    public void Create_WithValidCustomerName_ShouldCreateOrder()
    {
        // Arrange
        var customerName = "John Doe";

        // Act
        var order = Order.Create(customerName);

        // Assert
        order.Should().NotBeNull();
        order.CustomerName.Should().Be(customerName);
        order.Status.Should().Be(OrderStatus.Created);
        order.Items.Should().BeEmpty();

        // Generated inside domain: assert "set", not exact value
        order.Id.Should().NotBeNull();
        order.CreatedAt.Should().NotBe(default);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCustomerName_ShouldThrowArgumentException(string customerName)
    {
        // Act
        var act = () => Order.Create(customerName);

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
    public void AddItem_WhenOrderIsNotCreated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        order.Confirm(); // now status != Created

        // Act
        var act = () => order.AddItem("Product B", 1, Money.Create(5m, "EUR"));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Created state*");
    }

    [Fact]
    public void Confirm_WhenOrderHasNoItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Empty().Build();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
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
    public void Confirm_WhenOrderIsNotCreated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        order.Confirm(); // now Confirmed

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Created state*");
    }

    [Fact]
    public void Ship_WhenOrderIsConfirmed_ShouldChangeStatusToShipped()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        order.Confirm();

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_WhenOrderIsNotConfirmed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.WithOneItem().Build();
        // status is Created

        // Act
        var act = () => order.Ship();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Confirmed*");
    }
}
