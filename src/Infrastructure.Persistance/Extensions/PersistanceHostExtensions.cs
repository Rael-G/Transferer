using Api.Data.Repositories;
using Api.Data.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Infrastructure.Persistance.Services;
using Microsoft.AspNetCore.Builder;

namespace Api.Extensions
{
    public static class PersistanceHostExtensions
    {
        public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TransfererDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

            var filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage");

            if (!Directory.Exists(filesDirectory))
                Directory.CreateDirectory(filesDirectory);
            

            services.AddScoped<IFileStorage>(provider =>
                new LocalFileStorage(filesDirectory));
            services.AddScoped<IArchiveRepository, ArchiveRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
        }

        public static void InitializeDb(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<TransfererDbContext>();
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            SeederService.Seed(roleManager, userManager);
        }
    }
}
