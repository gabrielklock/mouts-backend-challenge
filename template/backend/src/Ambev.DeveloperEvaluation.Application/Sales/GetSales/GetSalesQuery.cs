using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public record GetSalesQuery(
    int Page = 1,
    int Size = 10,
    string? Order = null,
    DateTime? MinDate = null,
    DateTime? MaxDate = null) : IRequest<GetSalesResult>;
