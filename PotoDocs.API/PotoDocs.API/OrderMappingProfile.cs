using AutoMapper;
using PotoDocs.API.Models;

namespace PotoDocs.API;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, AdminOrderDto>()
            .ForMember(o => o.DriverName, a => a.MapFrom(a => a.Driver.FirstName))
            .ForMember(o => o.DriverLastname, a => a.MapFrom(a => a.Driver.LastName));

        CreateMap<Order, DriverOrderDto>()
            .ForMember(o => o.DriverName, a => a.MapFrom(a => a.Driver.FirstName))
            .ForMember(o => o.DriverLastname, a => a.MapFrom(a => a.Driver.LastName));
    }
}
