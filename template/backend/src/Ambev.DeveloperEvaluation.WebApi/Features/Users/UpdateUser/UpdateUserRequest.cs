using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
    public UpdateUserNameRequest Name { get; set; } = new();
    public UpdateUserAddressRequest Address { get; set; } = new();
}

public class UpdateUserNameRequest
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}

public class UpdateUserAddressRequest
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Zipcode { get; set; } = string.Empty;
    public UpdateUserGeolocationRequest Geolocation { get; set; } = new();
}

public class UpdateUserGeolocationRequest
{
    public string Lat { get; set; } = string.Empty;
    public string Long { get; set; } = string.Empty;
}
