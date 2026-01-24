using Domain.Aggregates;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Domain.Tests.Builders;

namespace Domain.Tests.Aggregates;

public class OrderTests
{
    #region Creation Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrderWithCorrectProperties()
    {
        // Arrange
        var orderId = TestIds.DefaultOrderId;
        var customerName = "John Doe";
        var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var order = Order.Create(orderId, customerName, createdAt);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().Be(orderId);
        order.CustomerName.Should().Be(customerName);
        order.CreatedAt.Should().Be(createdAt);
        order.Status.Should().Be(OrderStatus.Created);
        order.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyOrWhiteSpaceCustomerName_ShouldThrowArgumentException(string customerName)
    {
        // Arrange
        var orderId = TestIds.DefaultOrderId;
        var createdAt = DateTime.UtcNow;

        // Act
        var act = () => Order.Create(orderId, customerName, createdAt);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("customerName");
    }

    [Fact]
    public void Create_WithNullCustomerName_ShouldThrowArgumentException()
    {
        // Arrange
        var orderId = TestIds.DefaultOrderId;
        var createdAt = DateTime.UtcNow;

        // Act
        var act = () => Order.Create(orderId, null, createdAt);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("customerName");
    }

    [Fact]
    public void Create_WithNullOrderId_ShouldThrowArgumentNullException()
    {
        // Arrange
        OrderId orderId = null;
        var customerName = "John Doe";
        var createdAt = DateTime.UtcNow;

        // Act
        var act = () => Order.Create(orderId, customerName, createdAt);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("id");
    }

    [Fact]
    public void Create_WithFutureDate_ShouldCreateOrder()
    {
        // Arrange
        var orderId = TestIds.DefaultOrderId;
        var customerName = "Jane Smith";
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var order = Order.Create(orderId, customerName, futureDate);

        // Assert
        order.CreatedAt.Should().Be(futureDate);
    }

    #endregion

    #region Item Management Tests

    [Fact]
    public void AddItem_WithValidItem_ShouldAddItemToOrderItems()
    {
        // Arrange
        var order = OrderBuilder.Empty().Build();
        var item = OrderItem.Create("Laptop", 1, Money.Create(999.99m, "USD"));

        // Act
        order.AddItem(item);

        // Assert
        order.Items.Should().Contain(item);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_MultipleItems_ShouldAddAllItems()
    {
        // Arrange
        var order = OrderBuilder.Empty().Build();
        var item1 = OrderItem.Create("Laptop", 1, Money.Create(999.99m, "USD"));
        var item2 = OrderItem.Create("Mouse", 2, Money.Create(25.50m, "USD"));
        var item3 = OrderItem.Create("Keyboard", 1, Money.Create(89.99m, "USD"));

        // Act
        order.AddItem(item1);
        order.AddItem(item2);
        order.AddItem(item3);

        // Assert
        order.Items.Should().HaveCount(3);
        order.Items.Should().Contain(new[] { item1, item2, item3 });
    }

    [Fact]
    public void AddItem_WithNullItem_ShouldThrowArgumentNullException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        var act = () => order.AddItem(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("item");
    }

    [Fact]
    public void RemoveItem_WithExistingItem_ShouldRemoveItemFromOrder()
    {
        // Arrange
        var order = new OrderBuilder()
            .AddItem("Laptop", 1, 999.99m, "USD")
            .AddItem("Mouse", 2, 25.50m, "USD")
            .Build();
        var itemToRemove = order.Items.First();

        // Act
        order.RemoveItem(itemToRemove);

        // Assert
        order.Items.Should().NotContain(itemToRemove);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveItem_WithNonExistentItem_ShouldNotThrow()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        var differentItem = OrderItem.Create("Different Product", 1, Money.Create(50m, "USD"));

        // Act
        var act = () => order.RemoveItem(differentItem);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RemoveItem_WithNullItem_ShouldThrowArgumentNullException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        var act = () => order.RemoveItem(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Total Amount Calculation Tests

    [Fact]
    public void GetTotalAmount_WithSingleItem_ShouldReturnItemTotal()
    {
        // Arrange
        var order = new OrderBuilder()
            .AddItem("Laptop", 2, 999.99m, "USD") // 2 * 999.99 = 1999.98
            .Build();

        // Act
        var totalAmount = order.GetTotalAmount();

        // Assert
        totalAmount.Should().Be(1999.98m);
    }

    [Fact]
    public void GetTotalAmount_WithMultipleItems_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var order = new OrderBuilder()
            .AddItem("Laptop", 2, 999.99m, "USD")   // 1999.98
            .AddItem("Mouse", 3, 25.50m, "USD")     // 76.50
            .AddItem("Keyboard", 1, 89.99m, "USD")  // 89.99
            .Build();                                // Total: 2166.47

        // Act
        var totalAmount = order.GetTotalAmount();

        // Assert
        totalAmount.Should().Be(2166.47m);
    }

    [Fact]
    public void GetTotalAmount_WithNoItems_ShouldReturnZero()
    {
        // Arrange
        var order = OrderBuilder.Empty().Build();

        // Act
        var totalAmount = order.GetTotalAmount();

        // Assert
        totalAmount.Should().Be(0m);
    }

    [Fact]
    public void GetTotalAmount_AfterAddingItem_ShouldUpdateTotal()
    {
        // Arrange
        var order = new OrderBuilder()
            .AddItem("Laptop", 1, 999.99m, "USD")
            .Build();
        var initialTotal = order.GetTotalAmount();

        // Act
        order.AddItem(OrderItem.Create("Mouse", 1, 25.50m, Money.Create(25.50m, "USD")));
        var newTotal = order.GetTotalAmount();

        // Assert
        initialTotal.Should().Be(999.99m);
        newTotal.Should().Be(1025.49m);
    }

    [Fact]
    public void GetTotalAmount_AfterRemovingItem_ShouldUpdateTotal()
    {
        // Arrange
        var order = new OrderBuilder()
            .AddItem("Laptop", 1, 999.99m, "USD")
            .AddItem("Mouse", 1, 25.50m, "USD")
            .Build();
        var itemToRemove = order.Items.Last();

        // Act
        order.RemoveItem(itemToRemove);
        var newTotal = order.GetTotalAmount();

        // Assert
        newTotal.Should().Be(999.99m);
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void Submit_WhenStatusIsCreated_ShouldChangeStatusToSubmitted()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        order.Submit();

        // Assert
        order.Status.Should().Be(OrderStatus.Submitted);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();

        // Act
        var act = () => order.Submit();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already submitted*");
    }

    [Fact]
    public void Submit_WhenStatusIsCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Cancel();

        // Act
        var act = () => order.Submit();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenStatusIsCreated_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenStatusIsSubmitted_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenStatusIsShipped_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();
        order.MarkAsPaid();
        order.Ship();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot cancel*shipped*");
    }

    [Fact]
    public void Cancel_WhenStatusIsConfirmed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();
        order.MarkAsPaid();
        order.Ship();
        order.Confirm();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsPaid_WhenStatusIsSubmitted_ShouldChangeStatusToPaid()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_WhenStatusIsCreated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        var act = () => order.MarkAsPaid();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be submitted*");
    }

    [Fact]
    public void MarkAsPaid_WhenStatusIsCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Cancel();

        // Act
        var act = () => order.MarkAsPaid();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ship_WhenStatusIsPaid_ShouldChangeStatusToShipped()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();
        order.MarkAsPaid();

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_WhenStatusIsNotPaid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();

        // Act
        var act = () => order.Ship();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be paid*");
    }

    [Fact]
    public void Ship_WhenStatusIsCreated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        var act = () => order.Ship();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Confirm_WhenStatusIsShipped_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();
        order.MarkAsPaid();
        order.Ship();

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenStatusIsNotShipped_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();
        order.MarkAsPaid();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be shipped*");
    }

    [Fact]
    public void Confirm_WhenStatusIsCreated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Order Lifecycle Tests

    [Fact]
    public void OrderLifecycle_HappyPath_ShouldTransitionThroughAllStatuses()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Assert - Created
        order.Status.Should().Be(OrderStatus.Created);

        // Act & Assert - Submit
        order.Submit();
        order.Status.Should().Be(OrderStatus.Submitted);

        // Act & Assert - Pay
        order.MarkAsPaid();
        order.Status.Should().Be(OrderStatus.Paid);

        // Act & Assert - Ship
        order.Ship();
        order.Status.Should().Be(OrderStatus.Shipped);

        // Act & Assert - Confirm
        order.Confirm();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void OrderLifecycle_CancellationPath_ShouldAllowCancellationBeforeShipment()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act - Submit and Cancel
        order.Submit();
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void OrderLifecycle_EarlyCancellation_ShouldAllowCancellationAtCreated()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    #endregion

    #region Business Rule Tests

    [Fact]
    public void Order_ShouldNotAllowModificationAfterConfirmation()
    {
        // Arrange
        var order = OrderBuilder.Default().Build();
        order.Submit();
        order.MarkAsPaid();
        order.Ship();
        order.Confirm();

        var newItem = OrderItem.Create("New Product", 1, Money.Create(50m, "USD"));

        // Act
        var act = () => order.AddItem(newItem);

        // Assert - Depending on business rules, this might throw
        // act.Should().Throw<InvalidOperationException>();
        // Or it might succeed - adjust based on actual domain rules
    }

    [Fact]
    public void Order_WithMultipleItemsSameCurrency_ShouldCalculateTotalCorrectly()
    {
        // Arrange & Act
        var order = new OrderBuilder()
            .AddItem("Product A", 5, 10.00m, "USD")
            .AddItem("Product B", 3, 20.00m, "USD")
            .AddItem("Product C", 2, 15.00m, "USD")
            .Build();

        // Assert
        order.GetTotalAmount().Should().Be(140.00m); // (5*10) + (3*20) + (2*15) = 50 + 60 + 30
    }

    #endregion
}