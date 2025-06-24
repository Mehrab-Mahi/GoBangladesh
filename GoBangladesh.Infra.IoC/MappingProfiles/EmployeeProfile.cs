using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using AutoMapper;

namespace GoBangladesh.Infra.IoC.MappingProfiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeVm>()
            .ForMember(x => x.Id, x => x.MapFrom(x => x.Id))
            .ForMember(x => x.CreatedBy, x => x.MapFrom(x => x.CreatedBy))
            .ForMember(x => x.CreateTime, x => x.MapFrom(x => x.CreateTime))
            .ForMember(x => x.LastModifiedBy, x => x.MapFrom(x => x.LastModifiedBy))
            .ForMember(x => x.LastModifiedTime, x => x.MapFrom(x => x.LastModifiedTime))
            .ForMember(x => x.Address, x => x.MapFrom(x => x.Address))
            .ForMember(x => x.CurrentEmployee, x => x.MapFrom(x => x.CurrentEmployee))
            .ForMember(x => x.DepartmentId, x => x.MapFrom(x => x.DepartmentId))
            .ForMember(x => x.DesignationId, x => x.MapFrom(x => x.DesignationId))
            .ForMember(x => x.Email, x => x.MapFrom(x => x.Email))
            .ForMember(x => x.EmployeeOfficeId, x => x.MapFrom(x => x.EmployeeOfficeId))
            .ForMember(x => x.FirstName, x => x.MapFrom(x => x.FirstName))
            .ForMember(x => x.ImageUrl, x => x.MapFrom(x => x.ImageUrl))
            .ForMember(x => x.IsUser, x => x.MapFrom(x => x.IsUser))
            .ForMember(x => x.JoiningDate, x => x.MapFrom(x => x.JoiningDate))
            .ForMember(x => x.LastName, x => x.MapFrom(x => x.LastName))
            .ForMember(x => x.LeavingDate, x => x.MapFrom(x => x.LeavingDate))
            .ForMember(x => x.PhoneNumber, x => x.MapFrom(x => x.PhoneNumber))
            .ForMember(x => x.UserName, x => x.MapFrom(x => x.UserName));

            CreateMap<EmployeeVm, Employee>()
            .ForMember(x => x.Id, x => x.Ignore())
            .ForMember(x => x.CreatedBy, x => x.Ignore())
            .ForMember(x => x.CreateTime, x => x.Ignore())
            .ForMember(x => x.LastModifiedBy, x => x.Ignore())
            .ForMember(x => x.LastModifiedTime, x => x.Ignore())
            .ForMember(x => x.Address, x => x.MapFrom(x => x.Address))
            .ForMember(x => x.CurrentEmployee, x => x.MapFrom(x => x.CurrentEmployee))
            .ForMember(x => x.DepartmentId, x => x.MapFrom(x => x.DepartmentId))
            .ForMember(x => x.DesignationId, x => x.MapFrom(x => x.DesignationId))
            .ForMember(x => x.Email, x => x.MapFrom(x => x.Email))
            .ForMember(x => x.EmployeeOfficeId, x => x.MapFrom(x => x.EmployeeOfficeId))
            .ForMember(x => x.FirstName, x => x.MapFrom(x => x.FirstName))
            .ForMember(x => x.ImageUrl, x => x.MapFrom(x => x.ImageUrl))
            .ForMember(x => x.IsUser, x => x.MapFrom(x => x.IsUser))
            .ForMember(x => x.JoiningDate, x => x.MapFrom(x => x.JoiningDate))
            .ForMember(x => x.LastName, x => x.MapFrom(x => x.LastName))
            .ForMember(x => x.LeavingDate, x => x.MapFrom(x => x.LeavingDate))
            .ForMember(x => x.PhoneNumber, x => x.MapFrom(x => x.PhoneNumber))
            .ForMember(x => x.UserName, x => x.MapFrom(x => x.UserName));
        }
    }
}