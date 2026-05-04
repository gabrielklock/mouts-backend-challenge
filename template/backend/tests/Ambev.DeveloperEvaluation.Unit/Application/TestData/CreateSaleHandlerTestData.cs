using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class CreateSaleHandlerTestData
{
    private static readonly Faker<CreateSaleItemCommand> _itemFaker = new Faker<CreateSaleItemCommand>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 3))
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(10, 500));

    private static readonly Faker<CreateSaleCommand> _commandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(c => c.SaleNumber, f => $"SALE-{f.Random.Number(1000, 9999)}")
        .RuleFor(c => c.SaleDate, f => f.Date.Recent(30))
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
        .RuleFor(c => c.BranchId, f => f.Random.Guid())
        .RuleFor(c => c.BranchName, f => $"Branch {f.Address.City()}")
        .RuleFor(c => c.Items, f => _itemFaker.Generate(f.Random.Int(1, 3)));

    public static CreateSaleCommand GenerateValidCommand() => _commandFaker.Generate();

    public static CreateSaleCommand GenerateCommandWithQuantity(int quantity) =>
        new Faker<CreateSaleCommand>()
            .RuleFor(c => c.SaleNumber, f => $"SALE-{f.Random.Number(1000, 9999)}")
            .RuleFor(c => c.SaleDate, f => f.Date.Recent(30))
            .RuleFor(c => c.CustomerId, f => f.Random.Guid())
            .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
            .RuleFor(c => c.BranchId, f => f.Random.Guid())
            .RuleFor(c => c.BranchName, f => $"Branch {f.Address.City()}")
            .RuleFor(c => c.Items, f => new List<CreateSaleItemCommand>
            {
                new() { ProductId = f.Random.Guid(), ProductName = f.Commerce.ProductName(), Quantity = quantity, UnitPrice = f.Random.Decimal(10, 500) }
            })
            .Generate();
}
