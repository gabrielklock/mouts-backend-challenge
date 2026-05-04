using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Applies dynamic ordering to an IQueryable based on a comma-separated order string.
    /// Format: "field asc, otherField desc" — field names are matched case-insensitively
    /// to entity property names.
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, string order)
    {
        var clauses = order
            .Trim('"')
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IOrderedQueryable<T>? ordered = null;

        foreach (var clause in clauses)
        {
            var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var fieldName = parts[0];
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            var property = typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Unknown sort field '{fieldName}' for type '{typeof(T).Name}'.");

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var keySelector = Expression.Lambda(propertyAccess, parameter);

            var methodName = (ordered is null, descending) switch
            {
                (true, false) => "OrderBy",
                (true, true) => "OrderByDescending",
                (false, false) => "ThenBy",
                (false, true) => "ThenByDescending"
            };

            var method = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.PropertyType);

            ordered = (IOrderedQueryable<T>)method.Invoke(null, [ordered ?? (object)source, keySelector])!;
        }

        return ordered ?? source;
    }

    /// <summary>
    /// Applies a wildcard string filter to the query.
    /// Supports: "value*" (starts with), "*value" (ends with), "*value*" (contains), "value" (exact).
    /// </summary>
    public static IQueryable<T> ApplyStringFilter<T>(
        this IQueryable<T> source,
        Expression<Func<T, string>> selector,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return source;

        var startsWithWildcard = value.StartsWith('*');
        var endsWithWildcard = value.EndsWith('*');
        var clean = value.Trim('*');

        Expression<Func<T, bool>> predicate;

        if (startsWithWildcard && endsWithWildcard)
            predicate = BuildStringPredicate(selector, clean, "Contains");
        else if (startsWithWildcard)
            predicate = BuildStringPredicate(selector, clean, "EndsWith");
        else if (endsWithWildcard)
            predicate = BuildStringPredicate(selector, clean, "StartsWith");
        else
            predicate = BuildStringPredicate(selector, value, "Equals");

        return source.Where(predicate);
    }

    private static Expression<Func<T, bool>> BuildStringPredicate<T>(
        Expression<Func<T, string>> selector,
        string value,
        string methodName)
    {
        var method = typeof(string).GetMethod(methodName, [typeof(string)])!;
        var body = Expression.Call(selector.Body, method, Expression.Constant(value));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }
}
