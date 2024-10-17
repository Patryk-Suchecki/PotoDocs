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
        CreateMap<Order, OrderDto>();

        // Mapowanie z TransportOrderDto na Order
        CreateMap<OrderDto, Order>();
    }
}
