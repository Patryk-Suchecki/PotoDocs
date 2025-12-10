using AutoMapper;
using PotoDocs.API.Entities;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<OrderFile, OrderFileDto>().ReverseMap();
            CreateMap<OrderStop, OrderStopDto>().ReverseMap();
            CreateMap<InvoiceItem, InvoiceItemDto>().ReverseMap();
            CreateMap<Invoice, InvoiceDto>().ReverseMap();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty));

            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore());

            CreateMap<Order, OrderDto>()
                .ReverseMap()
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());
        }
    }
}