namespace Ambev.DeveloperEvaluation.Domain.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public int Total { get; init; }
}
