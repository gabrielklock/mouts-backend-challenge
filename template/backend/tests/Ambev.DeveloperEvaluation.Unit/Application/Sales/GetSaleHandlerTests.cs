using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;
    private readonly Faker _faker = new();

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing sale id When getting sale Then returns mapped result")]
    public async Task Handle_ExistingId_ShouldReturnSale()
    {
        var sale = new Sale { Id = _faker.Random.Guid(), SaleNumber = "SALE-001" };
        var expected = new GetSaleResult { Id = sale.Id, SaleNumber = sale.SaleNumber };
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expected);

        var result = await _handler.Handle(new GetSaleQuery(sale.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(sale.Id);
    }

    [Fact(DisplayName = "Given non-existent sale id When getting sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentId_ShouldThrowKeyNotFoundException()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = async () => await _handler.Handle(new GetSaleQuery(_faker.Random.Guid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing sale id When getting sale Then calls repository once")]
    public async Task Handle_ExistingId_ShouldCallRepositoryOnce()
    {
        var id = _faker.Random.Guid();
        var sale = new Sale { Id = id };
        _saleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult());

        await _handler.Handle(new GetSaleQuery(id), CancellationToken.None);

        await _saleRepository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
