using Ambev.DeveloperEvaluation.Common.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Represents geographical coordinates with latitude and longitude.
/// This is a value object that encapsulates location information.
/// </summary>
public class Geolocation : ValueObjectBase
{
    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    public string Lat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    public string Long { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the Geolocation class.
    /// </summary>
    public Geolocation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the Geolocation class with specified coordinates.
    /// </summary>
    /// <param name="lat">The latitude coordinate</param>
    /// <param name="long">The longitude coordinate</param>
    public Geolocation(string lat, string @long)
    {
        Lat = lat;
        Long = @long;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Lat;
        yield return Long;
    }
}