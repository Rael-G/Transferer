using Application.Interfaces.Services;
using Application.Mappings;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class ApplicationHostExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            TokenService.SecretKey = configuration["Secrets:SecretKey"]
                ?? throw new ArgumentNullException("SecretKey", "Secret Key is not defined in appsettings.");

            services.AddAutoMapper(typeof(DomainToDto));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IArchiveService, ArchiveService>();
        }
    }
}
