using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.UpdateProduct;

public class UpdateProductProfile : Profile
{
    public UpdateProductProfile()
    {
        CreateMap<UpdateProductRequest, UpdateProductCommand>()
            .ForMember(dest => dest.RatingRate, opt => opt.MapFrom(src => src.RatingRate))
            .ForMember(dest => dest.RatingCount, opt => opt.MapFrom(src => src.RatingCount));
    }
}
