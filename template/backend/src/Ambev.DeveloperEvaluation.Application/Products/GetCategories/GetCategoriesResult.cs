namespace Ambev.DeveloperEvaluation.Application.Products.GetCategories;

public class GetCategoriesResult
{
    public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();
}
