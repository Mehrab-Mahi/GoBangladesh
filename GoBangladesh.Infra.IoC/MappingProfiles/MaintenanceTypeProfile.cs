using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using AutoMapper;

namespace GoBangladesh.Infra.IoC.MappingProfiles
{
    public class MaintenanceTypeProfile : Profile
    {
        public MaintenanceTypeProfile()
        {
            CreateMap<MaintenanceType, MaintenanceTypeVm>()
                  .ForMember(x => x.Id, x => x.MapFrom(x => x.Id))
                  .ForMember(x => x.CreatedBy, x => x.MapFrom(x => x.CreatedBy))
                  .ForMember(x => x.CreateTime, x => x.MapFrom(x => x.CreateTime))
                  .ForMember(x => x.LastModifiedBy, x => x.MapFrom(x => x.LastModifiedBy))
                  .ForMember(x => x.LastModifiedTime, x => x.MapFrom(x => x.LastModifiedTime))
                  .ForMember(x => x.Name, x => x.MapFrom(x => x.Name))
                  .ForMember(x => x.Description, x => x.MapFrom(x => x.Description));

            CreateMap<MaintenanceTypeVm, MaintenanceType>()
             .ForMember(x => x.Id, x => x.Ignore())
             .ForMember(x => x.CreatedBy, x => x.Ignore())
             .ForMember(x => x.CreateTime, x => x.Ignore())
             .ForMember(x => x.LastModifiedBy, x => x.Ignore())
             .ForMember(x => x.LastModifiedTime, x => x.Ignore())
             .ForMember(x => x.Name, x => x.MapFrom(x => x.Name))
             .ForMember(x => x.Description, x => x.MapFrom(x => x.Description));
        }
    }
}