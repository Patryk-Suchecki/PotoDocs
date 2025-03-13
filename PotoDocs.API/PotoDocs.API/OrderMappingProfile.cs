using AutoMapper;
using PotoDocs.API.Entities;
using PotoDocs.Shared.Models;

namespace PotoDocs.API;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Mapowanie Order → OrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(dto => dto.CMRFiles, opt => opt.MapFrom(src => src.CMRFiles.Select(cmr => cmr.Url).ToList()))
            .ForMember(dest => dest.Driver, opt => opt.MapFrom(src => src.Driver != null ? new UserDto
            {
                Email = src.Driver.Email,
                FirstName = src.Driver.FirstName,
                LastName = src.Driver.LastName
            } : null))
            .ForMember(dest => dest.Stops, opt => opt.MapFrom(src => src.Stops)) // ✅ Mapowanie listy przystanków
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company)) // ✅ Mapowanie firmy
            .ReverseMap(); // ✅ Odwrotne mapowanie

        // Mapowanie OrderStop → OrderStopDto
        CreateMap<OrderStop, OrderStopDto>().ReverseMap(); // ✅ Mapowanie przystanków

        // Mapowanie Company → CompanyDto
        CreateMap<Company, CompanyDto>().ReverseMap(); // ✅ Mapowanie firmy

        // Mapowanie User → UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
            .ReverseMap()
            .ForMember(dest => dest.Role, opt => opt.Ignore()); // ✅ Ignorujemy Role w mapowaniu DTO → Encja

        // Mapowanie OrderDto → Order
        CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.Driver, opt => opt.Ignore()) // ✅ Ignorujemy Driver, bo jest tylko Email w DTO
            .ForMember(dest => dest.CMRFiles, opt => opt.Ignore()) // ✅ CMRFiles muszą być obsługiwane osobno
            .ForMember(dest => dest.Stops, opt => opt.MapFrom(src => src.Stops)) // ✅ Mapowanie listy przystanków
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company)); // ✅ Mapowanie firmy
    }
}
