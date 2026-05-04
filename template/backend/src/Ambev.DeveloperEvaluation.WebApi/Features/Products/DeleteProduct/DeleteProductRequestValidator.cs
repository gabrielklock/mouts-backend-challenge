using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.DeleteProduct;

public class DeleteProductRequestValidator : AbstractValidator<DeleteProductRequest>
{
    public DeleteProductRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Product ID must be a valid non-empty GUID");
    }
}
