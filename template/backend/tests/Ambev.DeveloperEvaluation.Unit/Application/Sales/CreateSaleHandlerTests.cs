using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Policies;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<CreateSaleHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<CreateSaleProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateSaleHandler(_saleRepository, _mapper, _mediator, _logger);
    }

    [Fact(DisplayName = "Given valid command When creating sale Then returns created sale")]
    public async Task Handle_ValidCommand_ShouldReturnCreatedSale()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.Items.Should().HaveCount(command.Items.Count);
    }

    [Fact(DisplayName = "Given valid command When creating sale Then persists to repository")]
    public async Task Handle_ValidCommand_ShouldCallRepository()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid command When creating sale Then publishes SaleCreated event")]
    public async Task Handle_ValidCommand_ShouldPublishSaleCreatedEvent()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Theory(DisplayName = "Given quantity between 4 and 9 When creating sale Then applies 10% discount")]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public async Task Handle_QuantityBetweenFourAndNine_ShouldApplyTenPercentDiscount(int quantity)
    {
        var command = CreateSaleHandlerTestData.GenerateCommandWithQuantity(quantity);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Items.First().Discount.Should().Be(DiscountPolicy.TenPercentDiscount);
    }

    [Theory(DisplayName = "Given quantity between 10 and 20 When creating sale Then applies 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public async Task Handle_QuantityBetweenTenAndTwenty_ShouldApplyTwentyPercentDiscount(int quantity)
    {
        var command = CreateSaleHandlerTestData.GenerateCommandWithQuantity(quantity);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Items.First().Discount.Should().Be(DiscountPolicy.TwentyPercentDiscount);
    }

    [Fact(DisplayName = "Given quantity above 20 When creating sale Then throws ValidationException")]
    public async Task Handle_QuantityAboveTwenty_ShouldThrow()
    {
        var command = CreateSaleHandlerTestData.GenerateCommandWithQuantity(21);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given empty items When creating sale Then throws ValidationException")]
    public async Task Handle_EmptyItems_ShouldThrowValidationException()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items.Clear();

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given empty sale number When creating sale Then throws ValidationException")]
    public async Task Handle_EmptySaleNumber_ShouldThrowValidationException()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.SaleNumber = string.Empty;

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
