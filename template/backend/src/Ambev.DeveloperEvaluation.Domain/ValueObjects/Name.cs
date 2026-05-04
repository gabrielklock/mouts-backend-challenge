using Ambev.DeveloperEvaluation.Common.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Represents a person's name with first and last name components.
/// This is a value object that encapsulates name-related information.
/// </summary>
public class Name : ValueObjectBase
{
    /// <summary>
    /// Gets or sets the person's first name.
    /// </summary>
    public string Firstname { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the person's last name.
    /// </summary>
    public string Lastname { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full name by combining first and last name.
    /// </summary>
    public string FullName => $"{Firstname} {Lastname}".Trim();

    /// <summary>
    /// Initializes a new instance of the Name class.
    /// </summary>
    public Name()
    {
    }

    /// <summary>
    /// Initializes a new instance of the Name class with specified first and last names.
    /// </summary>
    /// <param name="firstname">The first name</param>
    /// <param name="lastname">The last name</param>
    public Name(string firstname, string lastname)
    {
        Firstname = firstname;
        Lastname = lastname;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Firstname;
        yield return Lastname;
    }
}