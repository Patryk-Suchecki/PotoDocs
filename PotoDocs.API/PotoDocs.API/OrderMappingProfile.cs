using AutoMapper;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, TransportOrderDto>()
            .ForMember(o => o.Driver, a => a.MapFrom(a => a.Driver.FirstName + a.Driver.LastName));
    }
}
