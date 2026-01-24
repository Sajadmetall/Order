using Domain.Orders;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "USD";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_WithNonPositiveAmount_ShouldThrowArgumentException(decimal amount)
    {
        // Act
        var act = () => Money.Create(amount, "USD");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCurrency_ShouldThrowArgumentException(string currency)
    {
        // Act
        var act = () => Money.Create(100m, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var a = Money.Create(100m, "USD");
        var b = Money.Create(100m, "USD");

        // Assert
        a.Should().Be(b);
        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        var a = Money.Create(100m, "USD");
        var b = Money.Create(200m, "USD");

        // Assert
        a.Should().NotBe(b);
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithDifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var a = Money.Create(100m, "USD");
        var b = Money.Create(100m, "EUR");

        // Assert
        a.Should().NotBe(b);
        a.Equals(b).Should().BeFalse();
    }
}
