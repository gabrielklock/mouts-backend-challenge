using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

public class CancelSaleItemRequestValidator : AbstractValidator<CancelSaleItemRequest>
{
    public CancelSaleItemRequestValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("Sale ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Sale ID must be a valid non-empty GUID");

        RuleFor(x => x.ItemId)
            .NotEmpty()
            .WithMessage("Item ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Item ID must be a valid non-empty GUID");
    }
}
