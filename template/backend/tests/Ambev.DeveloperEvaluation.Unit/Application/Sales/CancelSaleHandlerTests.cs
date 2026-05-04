using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelSaleHandler> _logger;
    private readonly CancelSaleHandler _handler;
    private readonly Faker _faker = new();

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<CancelSaleHandler>>();
        _handler = new CancelSaleHandler(_saleRepository, _mediator, _logger);
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

    [Fact(DisplayName = "Given existing active sale When cancelling Then sets IsCancelled true")]
    public async Task Handle_ActiveSale_ShouldCancelIt()
    {
        var sale = CreateActiveSale();
        var command = new CancelSaleCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        sale.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given existing sale When cancelling Then persists update")]
    public async Task Handle_ActiveSale_ShouldCallUpdateRepository()
    {
        var sale = CreateActiveSale();
        var command = new CancelSaleCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        await _saleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s => s.IsCancelled), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given existing sale When cancelling Then publishes SaleCancelled event")]
    public async Task Handle_ActiveSale_ShouldPublishEvent()
    {
        var sale = CreateActiveSale();
        var command = new CancelSaleCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new CancelSaleCommand(_faker.Random.Guid());
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given already cancelled sale When cancelling Then throws InvalidOperationException")]
    public async Task Handle_AlreadyCancelledSale_ShouldThrowInvalidOperationException()
    {
        var sale = CreateActiveSale();
        sale.Cancel();
        var command = new CancelSaleCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }
}
