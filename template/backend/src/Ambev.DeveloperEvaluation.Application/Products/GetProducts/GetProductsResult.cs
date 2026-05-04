using Ambev.DeveloperEvaluation.Application.Products.GetProduct;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProducts;

public class GetProductsResult
{
    public IEnumerable<GetProductResult> Data { get; set; } = Enumerable.Empty<GetProductResult>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
