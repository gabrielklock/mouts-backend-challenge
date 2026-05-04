using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesHandler : IRequestHandler<GetSalesQuery, GetSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _saleRepository.GetPagedAsync(
            request.Page, request.Size,
            request.Order,
            request.MinDate, request.MaxDate,
            cancellationToken);

        return new GetSalesResult
        {
            Data = _mapper.Map<IEnumerable<GetSaleResult>>(paged.Data),
            TotalItems = paged.Total,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(paged.Total / (double)request.Size)
        };
    }
}
