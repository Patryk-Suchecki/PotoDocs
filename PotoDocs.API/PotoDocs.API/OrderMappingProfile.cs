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
        CreateMap<Order, OrderDto>()
            .ForMember(dto => dto.CMRFiles, opt => opt.MapFrom(src => src.CMRFiles.Select(cmr => cmr.Url).ToList()))
            .ForMember(dest => dest.Driver, opt => opt.MapFrom(src => src.Driver != null ? new UserDto
            {
                Email = src.Driver.Email,
                FirstName = src.Driver.FirstName,
                LastName = src.Driver.LastName
            } : null));




        CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.Driver, opt => opt.Ignore());

        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();
    }
}
