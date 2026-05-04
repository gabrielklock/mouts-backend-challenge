using Ambev.DeveloperEvaluation.Common.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Represents a physical address with complete location information.
/// This is a value object that encapsulates address-related data.
/// </summary>
public class Address : ValueObjectBase
{
    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the street number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the postal/zip code.
    /// </summary>
    public string Zipcode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the geographical coordinates of the address.
    /// </summary>
    public Geolocation Geolocation { get; set; } = new();

    /// <summary>
    /// Gets the formatted address string.
    /// </summary>
    public string FormattedAddress => $"{Street}, {Number} - {City} - {Zipcode}".Trim();

    /// <summary>
    /// Initializes a new instance of the Address class.
    /// </summary>
    public Address()
    {
    }

    /// <summary>
    /// Initializes a new instance of the Address class with specified address components.
    /// </summary>
    /// <param name="city">The city name</param>
    /// <param name="street">The street name</param>
    /// <param name="number">The street number</param>
    /// <param name="zipcode">The postal/zip code</param>
    /// <param name="geolocation">The geographical coordinates</param>
    public Address(string city, string street, int number, string zipcode, Geolocation? geolocation = null)
    {
        City = city;
        Street = street;
        Number = number;
        Zipcode = zipcode;
        Geolocation = geolocation ?? new Geolocation();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return City;
        yield return Street;
        yield return Number;
        yield return Zipcode;
        yield return Geolocation;
    }
}
