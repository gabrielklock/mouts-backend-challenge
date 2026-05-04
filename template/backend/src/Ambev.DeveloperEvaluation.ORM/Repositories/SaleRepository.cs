using Ambev.DeveloperEvaluation.Common.Extensions;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Sale>> GetPagedAsync(
        int page, int size,
        string? order = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(minDate, maxDate);

        var total = await query.CountAsync(cancellationToken);

        var orderedQuery = string.IsNullOrWhiteSpace(order)
            ? query.OrderByDescending(s => s.CreatedAt)
            : query.ApplyOrdering<Sale>(order);

        var data = await orderedQuery
            .Include(s => s.Items)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResult<Sale> { Data = data, Total = total };
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<Sale> BuildFilterQuery(DateTime? minDate, DateTime? maxDate)
    {
        var query = _context.Sales.AsNoTracking();

        if (minDate.HasValue)
            query = query.Where(s => s.SaleDate >= minDate.Value);

        if (maxDate.HasValue)
            query = query.Where(s => s.SaleDate <= maxDate.Value);

        return query;
    }
}
