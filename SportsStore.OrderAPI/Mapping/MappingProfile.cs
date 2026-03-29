using AutoMapper;
using SportsStore.OrderAPI.Models;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderAPI.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId));

        CreateMap<CreateCustomerDto, Customer>();

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.ProductPrice));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : ""))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Email : ""))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<CheckoutDto, Order>()
            .ForMember(dest => dest.Line1, opt => opt.MapFrom(src => src.Line1))
            .ForMember(dest => dest.Line2, opt => opt.MapFrom(src => src.Line2))
            .ForMember(dest => dest.Line3, opt => opt.MapFrom(src => src.Line3))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.Zip))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.GiftWrap, opt => opt.MapFrom(src => src.GiftWrap));

        // Shipment mappings
        CreateMap<ShipmentRecord, ShipmentDto>()
            .ForMember(dest => dest.ShipmentId, opt => opt.MapFrom(src => src.ShipmentId));

        // Payment mappings
        CreateMap<PaymentRecord, PaymentRecordDto>()
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId));

        // Inventory mappings
        CreateMap<InventoryRecord, InventoryRecordDto>()
            .ForMember(dest => dest.RecordId, opt => opt.MapFrom(src => src.RecordId));
    }
}
