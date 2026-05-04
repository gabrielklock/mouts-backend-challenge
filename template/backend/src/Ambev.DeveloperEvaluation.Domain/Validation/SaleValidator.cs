using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(x => x.SaleNumber)
            .NotEmpty()
            .WithMessage("The sale number cannot be empty.")
            .MaximumLength(50)
            .WithMessage("The sale number cannot exceed 50 characters.");
        RuleFor(x => x.SaleDate)
            .NotEmpty()
            .WithMessage("The sale date cannot be empty.");
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("The customer ID cannot be empty.");
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("The customer name cannot be empty.")
            .MaximumLength(100)
            .WithMessage("The customer name cannot exceed 100 characters.");
        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage("The branch ID cannot be empty.");
        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage("The branch name cannot be empty.")
            .MaximumLength(100)
            .WithMessage("The branch name cannot exceed 100 characters.");
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Sale must have at least one item.");
        RuleForEach(x => x.Items)
            .SetValidator(new SaleItemValidator());
    }
}
