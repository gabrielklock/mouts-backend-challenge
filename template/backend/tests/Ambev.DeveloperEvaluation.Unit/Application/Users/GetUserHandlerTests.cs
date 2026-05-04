using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUserHandler _handler;
    private readonly Faker _faker = new();

    public GetUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetUserHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing user id When getting user Then returns mapped result")]
    public async Task Handle_ExistingId_ShouldReturnUser()
    {
        var user = new User { Id = _faker.Random.Guid(), Username = _faker.Internet.UserName() };
        var expected = new GetUserResult { Id = user.Id };
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(expected);

        var result = await _handler.Handle(new GetUserCommand(user.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }

    [Fact(DisplayName = "Given non-existent user id When getting user Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentId_ShouldThrowKeyNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = async () => await _handler.Handle(new GetUserCommand(_faker.Random.Guid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given empty id When getting user Then throws ValidationException")]
    public async Task Handle_EmptyId_ShouldThrowValidationException()
    {
        var act = async () => await _handler.Handle(new GetUserCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given existing user id When getting user Then calls repository once")]
    public async Task Handle_ExistingId_ShouldCallRepositoryOnce()
    {
        var user = new User { Id = _faker.Random.Guid() };
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(new GetUserResult());

        await _handler.Handle(new GetUserCommand(user.Id), CancellationToken.None);

        await _userRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }
}
