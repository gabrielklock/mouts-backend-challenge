using Ambev.DeveloperEvaluation.Common.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Represents a rating with a value and optional comment.
/// This is a value object that encapsulates rating-related data.
/// </summary>
public class Rating : ValueObjectBase
{
    /// <summary>
    /// Gets or sets the rating value.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the number of ratings.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Initializes a new instance of the Name class.
    /// </summary>
    public Rating()
    {
    }

    /// <summary>
    /// Initializes a new instance of the Rating class with specified rate and count.
    /// </summary>
    /// <param name="rate">The rating value</param>
    /// <param name="count">The number of ratings</param>
    public Rating(decimal rate, int count)
    {
        Rate = rate;
        Count = count;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Rate;
        yield return Count;
    }
}
