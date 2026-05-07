using App.Domain.DTOs;
using App.Domain.Entities;
using AutoMapper;

namespace App.Business.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(destination => destination.UserName, options => options.MapFrom(source => source.Email))
            .ForMember(destination => destination.FirstName, options => options.MapFrom(source => source.FirstName))
            .ForMember(destination => destination.LastName, options => options.MapFrom(source => source.LastName));
    }
}
