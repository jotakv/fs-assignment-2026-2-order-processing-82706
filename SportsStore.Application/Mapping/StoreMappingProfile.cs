using AutoMapper;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Products.Commands;
using SportsStore.Domain.Entities;

namespace SportsStore.Application.Mapping;

public sealed class StoreMappingProfile : Profile
{
    public StoreMappingProfile()
    {
        CreateMap<Product, ProductDto>();

        CreateMap<CartLine, OrderLineDto>()
            .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.Product.ProductID))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Product.Price * src.Quantity));

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Count != 0
                ? src.Items.Sum(item => item.LineTotal)
                : src.Lines.Sum(line => line.Product.Price * line.Quantity)))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count != 0
                ? src.Items.Sum(item => item.Quantity)
                : src.Lines.Sum(line => line.Quantity)));

        CreateMap<CreateProductCommand, Product>()
            .ForMember(dest => dest.ProductID, opt => opt.Ignore());
        CreateMap<UpdateProductCommand, Product>();
    }
}
