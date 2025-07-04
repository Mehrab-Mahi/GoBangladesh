using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Services;
using GoBangladesh.Domain.Interfaces;
using GoBangladesh.Infra.Data.Context;
using GoBangladesh.Infra.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GoBangladesh.Infra.IoC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IConnectionStringProvider, ConnectionStringProvider>();
            services.AddScoped<IBaseRepository, BaseRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ILoggedInUserService, LoggedInUserService>();
            services.AddScoped<IPassengerService, PassengerService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IBusService, BusService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ISessionService, SessionService>();
        }
    }
}