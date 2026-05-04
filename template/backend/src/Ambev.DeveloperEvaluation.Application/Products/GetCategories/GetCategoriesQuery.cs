using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetCategories;

public record GetCategoriesQuery : IRequest<GetCategoriesResult>;
