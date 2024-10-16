using AutoMapper;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, TransportOrderDto>()
            .ForMember(o => o.Driver, a => a.MapFrom(a => a.Driver.FirstName + a.Driver.LastName));
        CreateMap<TransportOrderDto, Order>()
            .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber.ToString()))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => (float)src.Price.Amount))
            .ForMember(dest => dest.LoadingAddress, opt => opt.MapFrom(src => src.LoadingAddress.Location))
            .ForMember(dest => dest.UnloadingAddress, opt => opt.MapFrom(src => src.UnloadingAddress.Location))
            .ForMember(dest => dest.InvoiceIssueDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.InvoiceDate)));
    }
}
