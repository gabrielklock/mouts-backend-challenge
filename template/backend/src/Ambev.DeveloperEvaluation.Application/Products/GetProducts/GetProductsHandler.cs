using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProducts;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<GetProductsResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _productRepository.GetPagedAsync(
            request.Page, request.Size,
            request.Order,
            request.Title, request.Category,
            request.MinPrice, request.MaxPrice,
            cancellationToken);

        return new GetProductsResult
        {
            Data = _mapper.Map<IEnumerable<GetProductResult>>(paged.Data),
            TotalItems = paged.Total,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(paged.Total / (double)request.Size)
        };
    }
}
