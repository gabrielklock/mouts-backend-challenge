using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Policies;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    private SaleItem() { }

    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = DiscountPolicy.Calculate(quantity);
        TotalAmount = quantity * unitPrice * (1 - Discount);
    }

    public void Cancel()
    {
        IsCancelled = true;
        TotalAmount = 0;
    }
}
