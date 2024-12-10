using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Mapowanie z Order na OrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(dto => dto.CMRFiles, opt => opt.MapFrom(src => src.CMRFiles.Select(cmr => cmr.Url).ToList()))
            .ForMember(dest => dest.Driver, opt => opt.MapFrom(src => src.Driver != null ? new UserDto
            {
                Email = src.Driver.Email,
                FirstName = src.Driver.FirstName,
                LastName = src.Driver.LastName
            } : null));

        // Mapowanie z OrderDto na Order
        CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.CMRFiles, opt => opt.Ignore());

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Role, opt => opt.Ignore());
    }
}
