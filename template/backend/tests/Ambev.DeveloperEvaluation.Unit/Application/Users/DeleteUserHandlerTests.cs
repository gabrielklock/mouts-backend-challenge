using FluentAssertions;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly DeleteUserHandler _handler;
    private readonly Faker _faker = new();

    public DeleteUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new DeleteUserHandler(_userRepository);
    }

    [Fact(DisplayName = "Given existing user id When deleting Then returns success")]
    public async Task Handle_ExistingUser_ShouldReturnSuccess()
    {
        var id = _faker.Random.Guid();
        _userRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given non-existent user id When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentUser_ShouldThrowKeyNotFoundException()
    {
        _userRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var act = async () => await _handler.Handle(new DeleteUserCommand(_faker.Random.Guid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given empty id When deleting Then throws ValidationException")]
    public async Task Handle_EmptyId_ShouldThrowValidationException()
    {
        var act = async () => await _handler.Handle(new DeleteUserCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given existing user id When deleting Then calls repository once")]
    public async Task Handle_ExistingUser_ShouldCallRepositoryOnce()
    {
        var id = _faker.Random.Guid();
        _userRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        await _handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        await _userRepository.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
