using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserHandler(IUserRepository userRepository, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UpdateUserResult> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {command.Id} not found.");

        user.Username = command.Username;
        user.Email = command.Email;
        user.Phone = command.Phone;
        user.Status = command.Status;
        user.Role = command.Role;
        user.Name.Firstname = command.Firstname;
        user.Name.Lastname = command.Lastname;
        user.Address.City = command.City;
        user.Address.Street = command.Street;
        user.Address.Number = command.AddressNumber;
        user.Address.Zipcode = command.Zipcode;
        user.Address.Geolocation.Lat = command.Lat;
        user.Address.Geolocation.Long = command.Long;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(command.Password))
            user.Password = _passwordHasher.HashPassword(command.Password);

        var updated = await _userRepository.UpdateAsync(user, cancellationToken);
        return _mapper.Map<UpdateUserResult>(updated);
    }
}
