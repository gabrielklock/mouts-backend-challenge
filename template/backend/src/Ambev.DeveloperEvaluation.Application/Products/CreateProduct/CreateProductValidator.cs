using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RatingRate).InclusiveBetween(0, 5);
        RuleFor(x => x.RatingCount).GreaterThanOrEqualTo(0);
    }
}
