namespace Ambev.DeveloperEvaluation.Domain.Policies;

public static class DiscountPolicy
{
    public const int MaxQuantityPerItem = 20;
    public const int MinQuantityForTenPercent = 4;
    public const int MinQuantityForTwentyPercent = 10;
    public const decimal TenPercentDiscount = 0.10m;
    public const decimal TwentyPercentDiscount = 0.20m;

    public static decimal Calculate(int quantity) => quantity switch
    {
        > MaxQuantityPerItem => throw new InvalidOperationException(
            $"Cannot sell more than {MaxQuantityPerItem} identical items."),
        >= MinQuantityForTwentyPercent => TwentyPercentDiscount,
        >= MinQuantityForTenPercent    => TenPercentDiscount,
        _                              => 0m
    };
}
