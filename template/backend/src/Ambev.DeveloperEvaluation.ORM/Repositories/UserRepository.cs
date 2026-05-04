using Ambev.DeveloperEvaluation.Common.Extensions;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DefaultContext _context;

    public UserRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<User>> GetPagedAsync(
        int page, int size,
        string? order = null,
        string? username = null,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(username, email);

        var total = await query.CountAsync(cancellationToken);

        var orderedQuery = string.IsNullOrWhiteSpace(order)
            ? query.OrderBy(u => u.Username)
            : query.ApplyOrdering<User>(order);

        var data = await orderedQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResult<User> { Data = data, Total = total };
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    private IQueryable<User> BuildFilterQuery(string? username, string? email)
    {
        var query = _context.Users.AsNoTracking();

        query = query.ApplyStringFilter(u => u.Username, username);

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email == email);

        return query;
    }
}
