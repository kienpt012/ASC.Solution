using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Services;
using ASC.Web.Configuration;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options; // Thêm namespace này

namespace ASC.Web.Services
{
    public static class DependencyInjection // Sửa thành static class
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            // Add AddDbContext with connectionString to mirage database
            var connectionString = config.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            // Add Options and get data from appsettings.json with "AppSettings"
            services.AddOptions<ApplicationSettings>().Bind(config.GetSection("AppSettings")); // Sửa Ioption thành IOptions và dùng Bind

            return services;
        }

        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services)
        {
            ///Add ApplicationDbContext
            services.AddScoped<DbContext, ApplicationDbContext>();

            ///Add IdentityUser
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            ///Add services
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IIdentitySeed, IdentitySeed>();

            ///---
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDistributedMemoryCache();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();
            ///Add RazorPages - MVC
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddControllersWithViews();

            return services;
        }
    }
}