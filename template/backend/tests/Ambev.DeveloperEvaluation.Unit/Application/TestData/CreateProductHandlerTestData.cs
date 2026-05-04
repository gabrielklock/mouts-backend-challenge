using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class CreateProductHandlerTestData
{
    private static readonly Faker<CreateProductCommand> _commandFaker = new Faker<CreateProductCommand>()
        .RuleFor(c => c.Title, f => f.Commerce.ProductName())
        .RuleFor(c => c.Price, f => f.Random.Decimal(1, 1000))
        .RuleFor(c => c.Description, f => f.Commerce.ProductDescription())
        .RuleFor(c => c.Category, f => f.Commerce.Department())
        .RuleFor(c => c.Image, f => f.Image.PicsumUrl())
        .RuleFor(c => c.RatingRate, f => Math.Round(f.Random.Decimal(1, 5), 2))
        .RuleFor(c => c.RatingCount, f => f.Random.Int(1, 5000));

    public static CreateProductCommand GenerateValidCommand() => _commandFaker.Generate();
}
