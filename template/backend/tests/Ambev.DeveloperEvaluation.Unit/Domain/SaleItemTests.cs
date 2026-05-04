using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public class SaleItemTests
{
    private static readonly Guid _productId = Guid.NewGuid();
    private const string ProductName = "Test Product";

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Constructor_WhenQuantityBelowFour_ShouldHaveNoDiscount(int quantity)
    {
        var item = new SaleItem(_productId, ProductName, quantity, 100m);

        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(quantity * 100m);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public void Constructor_WhenQuantityBetweenFourAndNine_ShouldApplyTenPercentDiscount(int quantity)
    {
        var item = new SaleItem(_productId, ProductName, quantity, 100m);

        item.Discount.Should().Be(DiscountPolicy.TenPercentDiscount);
        item.TotalAmount.Should().Be(quantity * 100m * (1 - DiscountPolicy.TenPercentDiscount));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Constructor_WhenQuantityBetweenTenAndTwenty_ShouldApplyTwentyPercentDiscount(int quantity)
    {
        var item = new SaleItem(_productId, ProductName, quantity, 100m);

        item.Discount.Should().Be(DiscountPolicy.TwentyPercentDiscount);
        item.TotalAmount.Should().Be(quantity * 100m * (1 - DiscountPolicy.TwentyPercentDiscount));
    }

    [Theory]
    [InlineData(21)]
    [InlineData(50)]
    public void Constructor_WhenQuantityAboveMaximum_ShouldThrowInvalidOperationException(int quantity)
    {
        var act = () => new SaleItem(_productId, ProductName, quantity, 100m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{DiscountPolicy.MaxQuantityPerItem}*");
    }

    [Fact]
    public void Constructor_ShouldSetAllPropertiesCorrectly()
    {
        var productId = Guid.NewGuid();
        var item = new SaleItem(productId, "My Product", 5, 200m);

        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("My Product");
        item.Quantity.Should().Be(5);
        item.UnitPrice.Should().Be(200m);
        item.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public void Constructor_TotalAmount_ShouldReflectDiscountApplied()
    {
        // 5 items * $200 * (1 - 10%) = $900
        var item = new SaleItem(_productId, ProductName, 5, 200m);

        item.TotalAmount.Should().Be(900m);
    }

    [Fact]
    public void Cancel_ShouldSetIsCancelledTrue()
    {
        var item = new SaleItem(_productId, ProductName, 5, 100m);

        item.Cancel();

        item.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldZeroOutTotalAmount()
    {
        var item = new SaleItem(_productId, ProductName, 5, 100m);

        item.Cancel();

        item.TotalAmount.Should().Be(0m);
    }
}
