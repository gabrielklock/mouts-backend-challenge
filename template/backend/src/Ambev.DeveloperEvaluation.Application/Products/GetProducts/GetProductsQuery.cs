using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int Size = 10,
    string? Order = null,
    string? Title = null,
    string? Category = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null) : IRequest<GetProductsResult>;
