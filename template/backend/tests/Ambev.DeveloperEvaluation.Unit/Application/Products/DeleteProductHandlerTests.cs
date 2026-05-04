using FluentAssertions;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products;

public class DeleteProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly DeleteProductHandler _handler;
    private readonly Faker _faker = new();

    public DeleteProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _handler = new DeleteProductHandler(_productRepository);
    }

    [Fact(DisplayName = "Given existing product id When deleting Then returns success")]
    public async Task Handle_ExistingProduct_ShouldReturnSuccess()
    {
        var id = _faker.Random.Guid();
        _productRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteProductCommand(id), CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given non-existent product id When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentProduct_ShouldThrowKeyNotFoundException()
    {
        _productRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var act = async () => await _handler.Handle(new DeleteProductCommand(_faker.Random.Guid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing product id When deleting Then calls repository once")]
    public async Task Handle_ExistingProduct_ShouldCallRepositoryOnce()
    {
        var id = _faker.Random.Guid();
        _productRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        await _handler.Handle(new DeleteProductCommand(id), CancellationToken.None);

        await _productRepository.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
