using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelSaleItemHandler> _logger;
    private readonly CancelSaleItemHandler _handler;
    private readonly Faker _faker = new();

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<CancelSaleItemHandler>>();
        _handler = new CancelSaleItemHandler(_saleRepository, _mediator, _logger);
    }

    private Sale CreateSaleWithItem(out SaleItem item)
    {
        var sale = new Sale
        {
            Id = _faker.Random.Guid(),
            SaleNumber = $"SALE-{_faker.Random.Number(1000, 9999)}",
            CustomerId = _faker.Random.Guid(),
            CustomerName = _faker.Company.CompanyName(),
            BranchId = _faker.Random.Guid(),
            BranchName = _faker.Address.City()
        };
        var saleItem = new SaleItem(_faker.Random.Guid(), _faker.Commerce.ProductName(), 2, 50m);
        sale.AddItem(saleItem);
        item = saleItem;
        return sale;
    }

    [Fact(DisplayName = "Given valid sale and item When cancelling item Then returns success")]
    public async Task Handle_ValidSaleAndItem_ShouldReturnSuccess()
    {
        var sale = CreateSaleWithItem(out var item);
        var command = new CancelSaleItemCommand(sale.Id, item.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given valid sale and item When cancelling item Then item is marked cancelled")]
    public async Task Handle_ValidSaleAndItem_ShouldCancelItem()
    {
        var sale = CreateSaleWithItem(out var item);
        var command = new CancelSaleItemCommand(sale.Id, item.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        item.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given valid sale and item When cancelling item Then publishes ItemCancelled event")]
    public async Task Handle_ValidSaleAndItem_ShouldPublishEvent()
    {
        var sale = CreateSaleWithItem(out var item);
        var command = new CancelSaleItemCommand(sale.Id, item.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<ItemCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ShouldThrowKeyNotFoundException()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        var command = new CancelSaleItemCommand(_faker.Random.Guid(), _faker.Random.Guid());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling item Then throws InvalidOperationException")]
    public async Task Handle_CancelledSale_ShouldThrowInvalidOperationException()
    {
        var sale = CreateSaleWithItem(out var item);
        sale.Cancel();
        var command = new CancelSaleItemCommand(sale.Id, item.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "Given non-existent item When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_ItemNotFound_ShouldThrowKeyNotFoundException()
    {
        var sale = CreateSaleWithItem(out _);
        var command = new CancelSaleItemCommand(sale.Id, _faker.Random.Guid());
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
