using GoBangladesh.Application.Helper;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Enums;
using GoBangladesh.Infra.Data.Context;
using GoBangladesh.Infra.IoC;
using GoBangladesh.Infra.Data.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text;
using GoBangladesh.Application.ViewModels.Transaction;

namespace GoBangladesh.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<GoBangladeshDbContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString(Enum.GetName(typeof(DbConnection), DbConnection.GoBangladeshConnection_Local))));

            services.AddCors(options =>
            {
                options.AddPolicy("AllowsAll", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddControllers();
            services.AddSwaggerGen();
            services.AddHttpContextAccessor();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });
            services.AddHttpClient();
            RegisterServices(services);
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.RequireAuthenticatedSignIn = true;
            })
             .AddJwtBearer(options =>
             {
                 options.RequireHttpsMetadata = false;
                 options.SaveToken = true;
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters = JwtAuthentication.GetValidatorParameters(key);
             });

            ConfigureAutomapper(services);

            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            services.Configure<OtpSettings>(Configuration.GetSection("OtpSettings"));
            services.Configure<DistanceMatrixApiSettings>(Configuration.GetSection("DistanceMatrixApiSettings"));
        }

        private void ConfigureAutomapper(IServiceCollection services)
        {
            AutomapperConfig.Config(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseRouting();
            app.UseAuthorization();
            
            app.UseSession();
            app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    var JWToken = context.Session.GetString("token");
                    if (!string.IsNullOrEmpty(JWToken))
                    {
                        context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
                    }
                }
                await next();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            DependencyContainer.RegisterServices(services);
        }
    }
}