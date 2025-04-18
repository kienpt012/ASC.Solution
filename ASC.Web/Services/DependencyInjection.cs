using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ASC.Business.Interfaces;
using ASC.Business;
using AutoMapper;

namespace ASC.Web.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            // Add AddDbContext with connectionString to mirage database
            var connectionString = config.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            // Add Options and get data from appsettings.json with "AppSettings"
            services.AddOptions<ApplicationSettings>().Bind(config.GetSection("AppSettings"));
            return services;
        }

        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services, IConfiguration config)
        {
            ///Add ApplicationDbContext
            services.AddScoped<DbContext, ApplicationDbContext>();
            ///Add IdentityUsera
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            ///Add services
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IIdentitySeed, IdentitySeed>();
            services.AddAuthentication()
                .AddGoogle(Options =>
                {
                    IConfigurationSection googleAuthNSection = config.GetSection("Authentication:Google");
                    Options.ClientId = config["Google:Identity:ClientId"];
                    Options.ClientSecret = config["Google:Identity:ClientSecret"]; // Fixed "ClientId" to "ClientSecret"
                });
            ///---
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDistributedMemoryCache();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
         //   services.AddScoped<IMasterDataCacheOperations, MasterDataCacheOperations>();
            services.AddAutoMapper(typeof(ApplicationDbContext));
         //   services.AddAutoMapper(typeof(MasterDataCache));

            ///Add RazorPages - MVC
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddControllersWithViews();
            return services;
        }
    }
}