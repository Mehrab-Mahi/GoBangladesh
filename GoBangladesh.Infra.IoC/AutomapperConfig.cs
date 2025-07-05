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
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}