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
            .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
            .ForMember(dest => dest.InventoryResult, opt => opt.Ignore())
            .ForMember(dest => dest.InventoryReference, opt => opt.Ignore())
            .ForMember(dest => dest.ShipmentCarrier, opt => opt.Ignore())
            .ForMember(dest => dest.ShipmentReference, opt => opt.Ignore())
            .ForMember(dest => dest.TrackingNumber, opt => opt.Ignore())
            .ForMember(dest => dest.FailureReason, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Count != 0
                ? src.Items.Sum(item => item.LineTotal)
                : src.Lines.Sum(line => line.Product.Price * line.Quantity)))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count != 0
                ? src.Items.Sum(item => item.Quantity)
                : src.Lines.Sum(line => line.Quantity)))
            .AfterMap((src, dest) =>
            {
                InventoryRecord? latestInventory = src.InventoryRecords
                    .OrderBy(record => record.ProcessedAtUtc ?? DateTime.MinValue)
                    .LastOrDefault();

                PaymentRecord? latestPayment = src.PaymentRecords
                    .OrderBy(record => record.ProcessedAtUtc ?? DateTime.MinValue)
                    .LastOrDefault();

                ShipmentRecord? latestShipment = src.ShipmentRecords
                    .OrderBy(record => record.CreatedAtUtc ?? DateTime.MinValue)
                    .LastOrDefault();

                dest.PaymentStatus = latestPayment?.Status ?? src.StripePaymentStatus;
                dest.InventoryResult = latestInventory is null
                    ? null
                    : latestInventory.Succeeded ? "Confirmed" : "Failed";
                dest.InventoryReference = latestInventory?.ReservationReference;
                dest.ShipmentCarrier = latestShipment?.Carrier;
                dest.ShipmentReference = latestShipment?.ShipmentReference;
                dest.TrackingNumber = latestShipment?.TrackingNumber;
                dest.FailureReason = latestShipment?.FailureReason
                    ?? latestPayment?.FailureReason
                    ?? latestInventory?.FailureReason;
            });

        CreateMap<CreateProductCommand, Product>()
            .ForMember(dest => dest.ProductID, opt => opt.Ignore());
        CreateMap<UpdateProductCommand, Product>();
    }
}
