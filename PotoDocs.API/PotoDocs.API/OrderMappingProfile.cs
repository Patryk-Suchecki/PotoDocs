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
        CreateMap<Order, TransportOrderDto>()
            .ForMember(dto => dto.Driver, opt => opt.MapFrom(src => src.Driver.FirstName + " " + src.Driver.LastName)) // Łączenie imienia i nazwiska
            .ForMember(dto => dto.Price, opt => opt.MapFrom(src => new Money { Amount = (decimal)src.PriceAmount, Currency = src.PriceCurrency })) // Mapowanie ceny z walutą
            .ForMember(dto => dto.LoadingAddress, opt => opt.MapFrom(src => new Address { Location = src.LoadingAddress })) // Mapowanie adresu załadunku
            .ForMember(dto => dto.UnloadingAddress, opt => opt.MapFrom(src => new Address { Location = src.UnloadingAddress })) // Mapowanie adresu rozładunku
            .ForMember(dto => dto.InvoiceDate, opt => opt.MapFrom(src => src.InvoiceIssueDate.ToDateTime(TimeOnly.MinValue))); // Mapowanie daty faktury

        // Mapowanie z TransportOrderDto na Order
        CreateMap<TransportOrderDto, Order>()
            .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber))
            .ForMember(dest => dest.PriceAmount, opt => opt.MapFrom(src => (float)src.Price.Amount)) // Mapowanie kwoty ceny
            .ForMember(dest => dest.PriceCurrency, opt => opt.MapFrom(src => src.Price.Currency)) // Mapowanie waluty
            .ForMember(dest => dest.LoadingAddress, opt => opt.MapFrom(src => src.LoadingAddress.Location)) // Mapowanie adresu załadunku
            .ForMember(dest => dest.UnloadingAddress, opt => opt.MapFrom(src => src.UnloadingAddress.Location)) // Mapowanie adresu rozładunku
            .ForMember(dest => dest.InvoiceIssueDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.InvoiceDate))) // Mapowanie daty faktury
            .ForMember(dest => dest.Driver, opt => opt.Ignore()); // Mapowanie drivera, można rozbudować logikę do szukania kierowcy w bazie
    }
}
