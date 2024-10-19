using AutoMapper;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Mapowanie z Order na TransportOrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(dto => dto.CMRFiles, opt => opt.MapFrom(src => src.CMRFiles.Select(cmr => cmr.Url).ToList()));

        // Mapowanie z TransportOrderDto na Order
        CreateMap<OrderDto, Order>();
    }
}
