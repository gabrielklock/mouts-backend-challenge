using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("The product ID cannot be empty.");
        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("The product name cannot be empty.")
            .MaximumLength(200)
            .WithMessage("The product name cannot exceed 200 characters.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Quantity must be between 1 and 20.");
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than 0.");
    }
}
