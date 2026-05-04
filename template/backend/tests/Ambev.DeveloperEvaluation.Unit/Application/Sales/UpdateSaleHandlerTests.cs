using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateSaleHandler> _logger;
    private readonly UpdateSaleHandler _handler;
    private readonly Faker _faker = new();

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<UpdateSaleHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<UpdateSaleProfile>());
        _mapper = config.CreateMapper();

        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _mediator, _logger);
    }

    private Sale CreateActiveSale() => new Sale
    {
        Id = _faker.Random.Guid(),
        SaleNumber = $"SALE-{_faker.Random.Number(1000, 9999)}",
        CustomerId = _faker.Random.Guid(),
        CustomerName = _faker.Company.CompanyName(),
        BranchId = _faker.Random.Guid(),
        BranchName = _faker.Address.City()
    };

    private UpdateSaleCommand BuildValidCommand(Guid saleId) => new Faker<UpdateSaleCommand>()
        .RuleFor(c => c.Id, saleId)
        .RuleFor(c => c.SaleNumber, f => $"SALE-{f.Random.Number(1000, 9999)}")
        .RuleFor(c => c.SaleDate, f => f.Date.Recent(30))
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
        .RuleFor(c => c.BranchId, f => f.Random.Guid())
        .RuleFor(c => c.BranchName, f => f.Address.City())
        .RuleFor(c => c.Items, f => new List<UpdateSaleItemCommand>
        {
            new() { ProductId = f.Random.Guid(), ProductName = f.Commerce.ProductName(), Quantity = f.Random.Int(1, 3), UnitPrice = f.Random.Decimal(10, 200) }
        })
        .Generate();

    [Fact(DisplayName = "Given valid command When updating sale Then returns updated result")]
    public async Task Handle_ValidCommand_ShouldReturnUpdatedSale()
    {
        var sale = CreateActiveSale();
        var command = BuildValidCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
    }

    [Fact(DisplayName = "Given valid command When updating sale Then publishes SaleModified event")]
    public async Task Handle_ValidCommand_ShouldPublishEvent()
    {
        var sale = CreateActiveSale();
        var command = BuildValidCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When updating Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ShouldThrowKeyNotFoundException()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        var command = BuildValidCommand(_faker.Random.Guid());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When updating Then throws InvalidOperationException")]
    public async Task Handle_CancelledSale_ShouldThrowInvalidOperationException()
    {
        var sale = CreateActiveSale();
        sale.Cancel();
        var command = BuildValidCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cancelled*");
    }

    [Fact(DisplayName = "Given empty sale number When updating Then throws ValidationException")]
    public async Task Handle_EmptySaleNumber_ShouldThrowValidationException()
    {
        var command = BuildValidCommand(_faker.Random.Guid());
        command.SaleNumber = string.Empty;

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
