using GoBangladesh.Infra.IoC.MappingProfiles;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace GoBangladesh.Infra.IoC
{
    public class AutomapperConfig
    {
        public static void Config(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new DepartmentProfile());
                mc.AddProfile(new DesignationProfile());
                mc.AddProfile(new AssetTypeProfile());
                mc.AddProfile(new AssetStatusProfile());
                mc.AddProfile(new MaintenanceTypeProfile());
                mc.AddProfile(new UserProfile());
                mc.AddProfile(new EmployeeProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}