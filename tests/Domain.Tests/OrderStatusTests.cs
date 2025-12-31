using Domain.Entities;

namespace Domain.Tests
{
    public class OrderStatusTests
    {
        [Fact]
        public void Confirm_FromCreated_ChangesStatusToConfirmed()
        {
            // Arrange
            var order = CreateValidOrder();

            // Act
            order.Confirm();

            // Assert
            Assert.Equal(OrderStatus.Confirmed, order.Status);
        }
        [Fact]
        public void Ship_FromCreated_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateValidOrder();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => order.Ship());
        }
        [Fact]
        public void Ship_FromConfirmed_ChangesStatusToShipped()
        {
            // Arrange
            var order = CreateValidOrder();
            order.Confirm();

            // Act
            order.Ship();

            // Assert
            Assert.Equal(OrderStatus.Shipped, order.Status);
        }
        private static Order CreateValidOrder()
        {
            return new Order(
                "Test Customer",
                new List<OrderItem>
                {
                new OrderItem("Product A", 1, 10)
                });
        }
    }
}
