using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetCategories;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProducts;
using Ambev.DeveloperEvaluation.Application.Products.GetProductsByCategory;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.CreateProduct;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.GetProduct;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.UpdateProduct;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ProductsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateProductResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateProductRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateProductCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);

        return Created(result, "Product created successfully");
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<GetProductsResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        [FromQuery] string? title = null,
        [FromQuery] string? category = null,
        [FromQuery] decimal? _minPrice = null,
        [FromQuery] decimal? _maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(_page, _size, _order, title, category, _minPrice, _maxPrice),
            cancellationToken);

        return Ok(result, "Products retrieved successfully");
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetCategoriesResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);

        return Ok(result, "Categories retrieved successfully");
    }

    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetProductsByCategoryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(
        [FromRoute] string category,
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        [FromQuery] decimal? _minPrice = null,
        [FromQuery] decimal? _maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProductsByCategoryQuery(category, _page, _size, _order, _minPrice, _maxPrice),
            cancellationToken);

        return Ok(result, "Products retrieved successfully");
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new GetProductRequest { Id = id };
        var validator = new GetProductRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var result = await _mediator.Send(new GetProductQuery(request.Id), cancellationToken);

        return Ok(_mapper.Map<GetProductResponse>(result), "Product retrieved successfully");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateProductResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateProductRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateProductCommand>(request);
        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result, "Product updated successfully");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new DeleteProductRequest { Id = id };
        var validator = new DeleteProductRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        await _mediator.Send(new Application.Products.DeleteProduct.DeleteProductCommand(request.Id), cancellationToken);

        return Ok("Product deleted successfully");
    }
}
