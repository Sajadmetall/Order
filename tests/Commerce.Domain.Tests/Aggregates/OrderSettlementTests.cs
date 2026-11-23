using Commerce.Domain.Common;
using Commerce.Domain.Orders;
using Xunit;

namespace Commerce.Domain.Tests.Aggregates;

public sealed class OrderSettlementTests
{
    [Fact]
    public void Confirmed_order_can_be_paid_and_the_same_result_can_be_replayed()
    {
        var order = Order.Create("Test customer");
        order.AddItem("Premium", 4, Money.Create(2.5m, "EUR"));
        order.Confirm();

        order.MarkPaid(Money.Create(10, "EUR"));
        order.MarkPaid(Money.Create(10, "EUR"));

        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Theory]
    [InlineData(9, "EUR")]
    [InlineData(10, "USD")]
    public void Payment_must_match_order_total_and_currency(decimal amount, string currency)
    {
        var order = Order.Create("Test customer");
        order.AddItem("Premium", 1, Money.Create(10, "EUR"));
        order.Confirm();

        Assert.Throws<ArgumentException>(() => order.MarkPaid(Money.Create(amount, currency)));
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Unconfirmed_order_cannot_be_paid()
    {
        var order = Order.Create("Test customer");
        order.AddItem("Premium", 1, Money.Create(10, "EUR"));

        Assert.Throws<InvalidOperationException>(() => order.MarkPaid(Money.Create(10, "EUR")));
    }

    [Fact]
    public void Order_cannot_mix_currencies_before_payment()
    {
        var order = Order.Create("Test customer");
        order.AddItem("Premium", 1, Money.Create(10, "EUR"));

        Assert.Throws<InvalidOperationException>(() =>
            order.AddItem("Other product", 1, Money.Create(10, "USD")));
        Assert.Single(order.Items);
    }
}
