using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public UpdateProductHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {command.Id} not found.");

        product.Title = command.Title;
        product.Price = command.Price;
        product.Description = command.Description;
        product.Category = command.Category;
        product.Image = command.Image;
        product.UpdatedAt = DateTime.UtcNow;

        if (command.RatingRate >= 0 && command.RatingRate <= 5)
        {
            product.Rating = new Rating
            {
                Rate = command.RatingRate,
                Count = command.RatingCount
            };
        }

        var updated = await _productRepository.UpdateAsync(product, cancellationToken);
        return _mapper.Map<UpdateProductResult>(updated);
    }
}
