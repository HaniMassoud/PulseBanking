using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Users.Common
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<IdentityUser, UserDto>();

            CreateMap<CreateUserDto, CustomIdentityUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => 0));

            CreateMap<UpdateUserDto, CustomIdentityUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}