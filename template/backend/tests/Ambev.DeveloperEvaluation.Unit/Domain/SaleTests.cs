using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public class SaleTests
{
    private static Sale CreateSale() => new Sale
    {
        Id = Guid.NewGuid(),
        SaleNumber = "SALE-001",
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "Test Customer",
        BranchId = Guid.NewGuid(),
        BranchName = "Test Branch"
    };

    private static SaleItem CreateItem(int quantity = 1, decimal unitPrice = 100m) =>
        new SaleItem(Guid.NewGuid(), "Product", quantity, unitPrice);

    [Fact]
    public void AddItem_ShouldAddItemToCollection()
    {
        var sale = CreateSale();

        sale.AddItem(CreateItem(quantity: 2));

        sale.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_ShouldSetSaleIdOnItem()
    {
        var sale = CreateSale();
        var item = CreateItem();

        sale.AddItem(item);

        item.SaleId.Should().Be(sale.Id);
    }

    [Fact]
    public void AddItem_ShouldRecalculateTotalAmount()
    {
        var sale = CreateSale();

        sale.AddItem(CreateItem(quantity: 5, unitPrice: 100m));

        // 5 * 100 * (1 - 10%) = 450
        sale.TotalAmount.Should().Be(450m);
    }

    [Fact]
    public void AddItem_WhenQuantityExceedsMaximum_ShouldThrow()
    {
        var sale = CreateSale();

        var act = () => sale.AddItem(CreateItem(quantity: DiscountPolicy.MaxQuantityPerItem + 1));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{DiscountPolicy.MaxQuantityPerItem}*");
    }

    [Fact]
    public void AddMultipleItems_TotalAmount_ShouldSumAllItems()
    {
        var sale = CreateSale();
        sale.AddItem(CreateItem(quantity: 4, unitPrice: 100m));  // 4 * 100 * 0.90 = 360
        sale.AddItem(CreateItem(quantity: 2, unitPrice: 50m));   // 2 *  50 * 1.00 = 100

        sale.TotalAmount.Should().Be(460m);
    }

    [Fact]
    public void Cancel_ShouldSetIsCancelledTrue()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldSetUpdatedAt()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void CancelItem_ShouldMarkItemAsCancelled()
    {
        var sale = CreateSale();
        var item = CreateItem(quantity: 4, unitPrice: 100m);
        sale.AddItem(item);

        sale.CancelItem(item.Id);

        sale.Items.First(i => i.Id == item.Id).IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void CancelItem_ShouldRecalculateTotalExcludingCancelledItem()
    {
        var sale = CreateSale();
        var itemA = CreateItem(quantity: 4, unitPrice: 100m);  // 360
        var itemB = CreateItem(quantity: 2, unitPrice: 50m);   // 100
        sale.AddItem(itemA);
        sale.AddItem(itemB);

        sale.CancelItem(itemA.Id);

        sale.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public void CancelItem_WhenItemNotFound_ShouldThrow()
    {
        var sale = CreateSale();

        var act = () => sale.CancelItem(Guid.NewGuid());

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void UpdateItems_ShouldReplaceAllItemsAndRecalculateTotal()
    {
        var sale = CreateSale();
        sale.AddItem(CreateItem(quantity: 4, unitPrice: 100m));

        sale.UpdateItems([CreateItem(quantity: 10, unitPrice: 50m)]);

        sale.Items.Should().HaveCount(1);
        sale.Items.First().Discount.Should().Be(DiscountPolicy.TwentyPercentDiscount);
        sale.TotalAmount.Should().Be(10 * 50m * (1 - DiscountPolicy.TwentyPercentDiscount));
    }

    [Fact]
    public void UpdateItems_ShouldSetUpdatedAt()
    {
        var sale = CreateSale();

        sale.UpdateItems([CreateItem()]);

        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void TotalAmount_ShouldExcludeCancelledItems()
    {
        var sale = CreateSale();
        var itemA = CreateItem(quantity: 1, unitPrice: 200m);  // 200, no discount
        var itemB = CreateItem(quantity: 2, unitPrice: 50m);   // 100, no discount
        sale.AddItem(itemA);
        sale.AddItem(itemB);

        sale.CancelItem(itemA.Id);

        sale.TotalAmount.Should().Be(100m);
    }
}
