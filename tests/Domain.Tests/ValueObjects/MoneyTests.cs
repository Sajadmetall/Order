using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class MoneyTests
{
    #region Creation Tests

    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldReturnMoneyInstance()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "USD";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Should().NotBeNull();
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(1, "USD")]
    [InlineData(0.01, "EUR")]
    [InlineData(999999.99, "GBP")]
    [InlineData(100, "JPY")]
    public void Create_WithVariousValidAmounts_ShouldCreateMoney(decimal amount, string currency)
    {
        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_WithNonPositiveAmount_ShouldThrowArgumentException(decimal amount)
    {
        // Arrange
        var currency = "USD";

        // Act
        var act = () => Money.Create(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithNullOrWhiteSpaceCurrency_ShouldThrowArgumentException(string currency)
    {
        // Arrange
        var amount = 100m;

        // Act
        var act = () => Money.Create(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        money1.Equals(money2).Should().BeTrue();
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAmount_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(200m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        money1.Equals(money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
        money1.Equals(money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var money = Money.Create(100m, "USD");

        // Act & Assert
        money.Equals(null).Should().BeFalse();
        (money == null).Should().BeFalse();
        (money != null).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithEqualMoney_ShouldReturnSameHashCode()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act
        var hash1 = money1.GetHashCode();
        var hash2 = money2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentMoney_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(200m, "USD");

        // Act
        var hash1 = money1.GetHashCode();
        var hash2 = money2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Operation Tests

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSumOfAmounts()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50.75m, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150.75m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "EUR");

        // Act
        var act = () => money1 + money2;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*currency*");
    }

    [Fact]
    public void Multiply_ByPositiveInteger_ShouldReturnMultipliedAmount()
    {
        // Arrange
        var money = Money.Create(25.50m, "USD");
        var multiplier = 4;

        // Act
        var result = money * multiplier;

        // Assert
        result.Amount.Should().Be(102m);
        result.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public void Multiply_ByVariousPositiveIntegers_ShouldReturnCorrectResult(int multiplier)
    {
        // Arrange
        var money = Money.Create(10m, "USD");

        // Act
        var result = money * multiplier;

        // Assert
        result.Amount.Should().Be(10m * multiplier);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_ByZero_ShouldThrowArgumentException()
    {
        // Arrange
        var money = Money.Create(25m, "USD");
        var multiplier = 0;

        // Act
        var act = () => money * multiplier;

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Multiply_ByNegativeInteger_ShouldThrowArgumentException()
    {
        // Arrange
        var money = Money.Create(25m, "USD");
        var multiplier = -2;

        // Act
        var act = () => money * multiplier;

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = Money.Create(100.50m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("100.50");
        result.Should().Contain("USD");
    }

    #endregion
}