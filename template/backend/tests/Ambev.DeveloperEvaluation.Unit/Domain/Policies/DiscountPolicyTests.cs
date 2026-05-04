using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Policies;

public class DiscountPolicyTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Calculate_WhenQuantityBelowMinimum_ShouldReturnNoDiscount(int quantity)
    {
        var discount = DiscountPolicy.Calculate(quantity);

        discount.Should().Be(0m);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public void Calculate_WhenQuantityBetweenFourAndNine_ShouldReturnTenPercent(int quantity)
    {
        var discount = DiscountPolicy.Calculate(quantity);

        discount.Should().Be(DiscountPolicy.TenPercentDiscount);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Calculate_WhenQuantityBetweenTenAndTwenty_ShouldReturnTwentyPercent(int quantity)
    {
        var discount = DiscountPolicy.Calculate(quantity);

        discount.Should().Be(DiscountPolicy.TwentyPercentDiscount);
    }

    [Fact]
    public void Calculate_WhenQuantityEqualsMinForTenPercent_ShouldReturnTenPercent()
    {
        var discount = DiscountPolicy.Calculate(DiscountPolicy.MinQuantityForTenPercent);

        discount.Should().Be(DiscountPolicy.TenPercentDiscount);
    }

    [Fact]
    public void Calculate_WhenQuantityEqualsMinForTwentyPercent_ShouldReturnTwentyPercent()
    {
        var discount = DiscountPolicy.Calculate(DiscountPolicy.MinQuantityForTwentyPercent);

        discount.Should().Be(DiscountPolicy.TwentyPercentDiscount);
    }

    [Fact]
    public void Calculate_WhenQuantityEqualsMaximum_ShouldReturnTwentyPercent()
    {
        var discount = DiscountPolicy.Calculate(DiscountPolicy.MaxQuantityPerItem);

        discount.Should().Be(DiscountPolicy.TwentyPercentDiscount);
    }

    [Theory]
    [InlineData(21)]
    [InlineData(50)]
    [InlineData(100)]
    public void Calculate_WhenQuantityExceedsMaximum_ShouldThrowInvalidOperationException(int quantity)
    {
        var act = () => DiscountPolicy.Calculate(quantity);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{DiscountPolicy.MaxQuantityPerItem}*");
    }

    [Fact]
    public void Calculate_BoundaryBetweenZeroAndTenPercent_ShouldDistinguishCorrectly()
    {
        var belowThreshold = DiscountPolicy.Calculate(DiscountPolicy.MinQuantityForTenPercent - 1);
        var atThreshold = DiscountPolicy.Calculate(DiscountPolicy.MinQuantityForTenPercent);

        belowThreshold.Should().Be(0m);
        atThreshold.Should().Be(DiscountPolicy.TenPercentDiscount);
    }

    [Fact]
    public void Calculate_BoundaryBetweenTenAndTwentyPercent_ShouldDistinguishCorrectly()
    {
        var belowThreshold = DiscountPolicy.Calculate(DiscountPolicy.MinQuantityForTwentyPercent - 1);
        var atThreshold = DiscountPolicy.Calculate(DiscountPolicy.MinQuantityForTwentyPercent);

        belowThreshold.Should().Be(DiscountPolicy.TenPercentDiscount);
        atThreshold.Should().Be(DiscountPolicy.TwentyPercentDiscount);
    }
}
