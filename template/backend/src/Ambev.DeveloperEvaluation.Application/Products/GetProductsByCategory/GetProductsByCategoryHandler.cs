using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProductsByCategory;

public class GetProductsByCategoryHandler : IRequestHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsByCategoryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var paged = await _productRepository.GetPagedAsync(
            request.Page, request.Size,
            request.Order,
            title: null,
            category: request.Category,
            request.MinPrice, request.MaxPrice,
            cancellationToken);

        return new GetProductsByCategoryResult
        {
            Data = _mapper.Map<IEnumerable<GetProductResult>>(paged.Data),
            TotalItems = paged.Total,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(paged.Total / (double)request.Size)
        };
    }
}
