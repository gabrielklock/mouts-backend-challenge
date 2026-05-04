using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<CreateProductProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateProductHandler(_productRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid command When creating product Then returns created product")]
    public async Task Handle_ValidCommand_ShouldReturnCreatedProduct()
    {
        var command = CreateProductHandlerTestData.GenerateValidCommand();
        _productRepository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Product>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.Price.Should().Be(command.Price);
        result.Category.Should().Be(command.Category);
    }

    [Fact(DisplayName = "Given valid command When creating product Then persists to repository")]
    public async Task Handle_ValidCommand_ShouldCallRepository()
    {
        var command = CreateProductHandlerTestData.GenerateValidCommand();
        _productRepository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Product>());

        await _handler.Handle(command, CancellationToken.None);

        await _productRepository.Received(1).CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given empty title When creating product Then throws ValidationException")]
    public async Task Handle_EmptyTitle_ShouldThrowValidationException()
    {
        var command = CreateProductHandlerTestData.GenerateValidCommand();
        command.Title = string.Empty;

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given zero price When creating product Then throws ValidationException")]
    public async Task Handle_ZeroPrice_ShouldThrowValidationException()
    {
        var command = CreateProductHandlerTestData.GenerateValidCommand();
        command.Price = 0;

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given negative price When creating product Then throws ValidationException")]
    public async Task Handle_NegativePrice_ShouldThrowValidationException()
    {
        var command = CreateProductHandlerTestData.GenerateValidCommand();
        command.Price = -10m;

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
