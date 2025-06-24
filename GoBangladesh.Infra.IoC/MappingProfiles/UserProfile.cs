using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using AutoMapper;

namespace GoBangladesh.Infra.IoC.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserVm>()
             .ForMember(x => x.Id, x => x.MapFrom(x => x.Id))
             .ForMember(x => x.EmailAddress, x => x.MapFrom(x => x.EmailAddress))
             .ForMember(x => x.IsSuperAdmin, x => x.MapFrom(x => x.IsSuperAdmin))
             .ForMember(x => x.FirstName, x => x.MapFrom(x => x.FirstName))
             .ForMember(x => x.LastName, x => x.MapFrom(x => x.LastName))
             .ForMember(x => x.UserName, x => x.MapFrom(x => x.UserName))
             //.ForMember(x => x.ImageUrl, x => x.MapFrom(x => x.ImageUrl))
             .ForMember(x => x.RoleId, x => x.MapFrom(x => x.RoleId))
             .ForMember(x => x.CreatedBy, x => x.MapFrom(x => x.CreatedBy))
             .ForMember(x => x.CreateTime, x => x.MapFrom(x => x.CreateTime))
             .ForMember(x => x.LastModifiedBy, x => x.MapFrom(x => x.LastModifiedBy))
             .ForMember(x => x.LastModifiedTime, x => x.MapFrom(x => x.LastModifiedTime))
             .ForMember(x => x.IsActive, x => x.MapFrom(x => x.IsActive));

            

            CreateMap<UserVm, User>()
             .ForMember(x => x.Id, x => x.MapFrom(x => x.Id))
             .ForMember(x => x.CreatedBy, x => x.Ignore())
             .ForMember(x => x.CreateTime, x => x.Ignore())
             .ForMember(x => x.LastModifiedBy, x => x.Ignore())
             .ForMember(x => x.LastModifiedTime, x => x.Ignore())
             .ForMember(x => x.EmailAddress, x => x.MapFrom(x => x.EmailAddress))
             .ForMember(x => x.IsSuperAdmin, x => x.MapFrom(x => x.IsSuperAdmin))
             .ForMember(x => x.FirstName, x => x.MapFrom(x => x.FirstName))
             .ForMember(x => x.LastName, x => x.MapFrom(x => x.LastName))
             .ForMember(x => x.UserName, x => x.MapFrom(x => x.UserName))
             .ForMember(x => x.ImageUrl, x => x.Ignore())
             .ForMember(x => x.RoleId, x => x.MapFrom(x => x.RoleId))
             .ForMember(x => x.IsActive, x => x.MapFrom(x => x.IsActive));
        }
    }
}