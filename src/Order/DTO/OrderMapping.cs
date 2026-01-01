using Order.Controllers;
using Domain.Orders;
namespace Order.DTO
{
    public static class OrderMapping
    {
        public static OrderResponse ToDto(Domain.Orders.Order order) // Or just 'Order' if using the using above
        {
            var currency = order.Items.FirstOrDefault()?.UnitPrice.Currency ?? "EUR";

            var items = order.Items.Select(i => new OrderItemResponse(
                Id: i.Id,
                ProductName: i.ProductName,
                Quantity: i.Quantity,
                UnitPriceAmount: i.UnitPrice.Amount,
                TotalPriceAmount: i.TotalPrice.Amount
            )).ToList();

            var totalAmount = items.Sum(x => x.TotalPriceAmount);

            return new OrderResponse(
                Id: order.Id.Value,
                CustomerName: order.CustomerName,
                CreatedAt: order.CreatedAt,
                Status: order.Status.ToString(),
                TotalAmount: totalAmount,
                Currency: currency,
                Items: items
            );
        }
        public static Domain.Orders.Order ToDomain(CreateOrderRequest dto)
        {
            var order = Domain.Orders.Order.Create(dto.CustomerName);

            foreach (var item in dto.Items)
            {
                var money = Money.Create(item.UnitPriceAmount, item.Currency);
                order.AddItem(item.ProductName, item.Quantity, money);
            }

            return order;
        }
    }
}
