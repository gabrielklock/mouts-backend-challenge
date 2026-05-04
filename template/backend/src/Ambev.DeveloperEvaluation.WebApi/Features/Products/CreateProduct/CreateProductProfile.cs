using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.CreateProduct;

public class CreateProductProfile : Profile
{
    public CreateProductProfile()
    {
        CreateMap<CreateProductRequest, CreateProductCommand>()
            .ForMember(dest => dest.RatingRate, opt => opt.MapFrom(src => src.RatingRate))
            .ForMember(dest => dest.RatingCount, opt => opt.MapFrom(src => src.RatingCount));

        CreateMap<CreateProductCommand, Product>()
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => new Rating
            {
                Rate = src.RatingRate,
                Count = src.RatingCount
            }));
    }
}
