using AutoMapper;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Roles.Common
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            CreateMap<CustomIdentityRole, RoleDto>();

            CreateMap<CreateRoleDto, CustomIdentityRole>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedName, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());

            CreateMap<UpdateRoleDto, CustomIdentityRole>()
                .ForMember(dest => dest.NormalizedName, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());
        }
    }
}
