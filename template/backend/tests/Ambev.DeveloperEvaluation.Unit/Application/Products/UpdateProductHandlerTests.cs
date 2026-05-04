using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products;

public class UpdateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly UpdateProductHandler _handler;
    private readonly Faker _faker = new();

    public UpdateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateProductHandler(_productRepository, _mapper);
    }

    private Product CreateProduct() => new Product
    {
        Id = _faker.Random.Guid(),
        Title = _faker.Commerce.ProductName(),
        Price = _faker.Random.Decimal(10, 500),
        Description = _faker.Commerce.ProductDescription(),
        Category = _faker.Commerce.Department(),
        Image = _faker.Image.PicsumUrl(),
        Rating = new Ambev.DeveloperEvaluation.Domain.ValueObjects.Rating { Rate = _faker.Random.Decimal(1, 5), Count = _faker.Random.Int(1, 1000) },
        CreatedAt = DateTime.UtcNow
    };

    private UpdateProductCommand BuildCommand(Guid id) => new Faker<UpdateProductCommand>()
        .RuleFor(c => c.Id, id)
        .RuleFor(c => c.Title, f => f.Commerce.ProductName())
        .RuleFor(c => c.Price, f => f.Random.Decimal(10, 500))
        .RuleFor(c => c.Description, f => f.Commerce.ProductDescription())
        .RuleFor(c => c.Category, f => f.Commerce.Department())
        .RuleFor(c => c.Image, f => f.Image.PicsumUrl())
        .RuleFor(c => c.RatingRate, f => f.Random.Decimal(1, 5))
        .RuleFor(c => c.RatingCount, f => f.Random.Int(1, 1000))
        .Generate();

    [Fact(DisplayName = "Given existing product When updating Then returns updated result")]
    public async Task Handle_ExistingProduct_ShouldReturnUpdatedResult()
    {
        var product = CreateProduct();
        var command = BuildCommand(product.Id);
        var expected = new UpdateProductResult { Id = product.Id, Title = command.Title };
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Product>());
        _mapper.Map<UpdateProductResult>(Arg.Any<Product>()).Returns(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given existing product When updating Then updates all fields")]
    public async Task Handle_ExistingProduct_ShouldUpdateFields()
    {
        var product = CreateProduct();
        var command = BuildCommand(product.Id);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Product>());
        _mapper.Map<UpdateProductResult>(Arg.Any<Product>()).Returns(new UpdateProductResult());

        await _handler.Handle(command, CancellationToken.None);

        product.Title.Should().Be(command.Title);
        product.Price.Should().Be(command.Price);
        product.Category.Should().Be(command.Category);
    }

    [Fact(DisplayName = "Given non-existent product When updating Then throws KeyNotFoundException")]
    public async Task Handle_ProductNotFound_ShouldThrowKeyNotFoundException()
    {
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var command = BuildCommand(_faker.Random.Guid());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing product When updating Then persists to repository")]
    public async Task Handle_ExistingProduct_ShouldCallUpdateRepository()
    {
        var product = CreateProduct();
        var command = BuildCommand(product.Id);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Product>());
        _mapper.Map<UpdateProductResult>(Arg.Any<Product>()).Returns(new UpdateProductResult());

        await _handler.Handle(command, CancellationToken.None);

        await _productRepository.Received(1).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
