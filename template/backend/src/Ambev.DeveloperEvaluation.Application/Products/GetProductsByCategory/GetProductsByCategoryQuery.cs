using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProductsByCategory;

public record GetProductsByCategoryQuery(
    string Category,
    int Page = 1,
    int Size = 10,
    string? Order = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null) : IRequest<GetProductsByCategoryResult>;
