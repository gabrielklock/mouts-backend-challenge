using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
        SaleDate = DateTime.UtcNow;
        IsCancelled = false;
    }

    public void AddItem(SaleItem item)
    {
        item.SaleId = Id;
        _items.Add(item);
        RecalculateTotalAmount();
    }

    public void Cancel()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item with ID {itemId} not found in sale");

        item.Cancel();
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItems(List<SaleItem> items)
    {
        _items.Clear();
        foreach (var item in items)
        {
            item.SaleId = Id;
            _items.Add(item);
        }
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }
}
