using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Api.Data.Contexts;
using Api.Services;

namespace Api
{
    public static class Settings
    {
        //TODO
        //Make _secret secret
        public static readonly string _secret = "secret? 50a1b6e3-bfdb-448f-850f-17ff478f833d";

        public static void ConfigureAPI(this IServiceCollection services)
        {
            //TODO: extrair a connection string para um metodo que crie o diretorio caso não exista. Deletar o .gitkeep do diretorio
            services.AddDbContext<TransfererDbContext>(options =>
                options.UseSqlite($"Data Source={Directory.GetCurrentDirectory()}\\Storage\\Db\\Transferer.db;"));

            services.AddScoped<IArchiveRepository, ArchiveRepository>();
            services.AddScoped<IFileStorage>(provider =>
                new LocalFileStorage($"{Directory.GetCurrentDirectory()}\\Storage\\Files"));
        }

        public static void ConfigureAuth(this IServiceCollection services)
        {
            services.AddScoped<UserContext, UserContext>();

            services.AddDbContext<UserContext>(
                options => options.UseSqlite(
                    $"Data Source={Directory.GetCurrentDirectory()}\\Storage\\Db\\Auth.db;"
            ));

            var key = Encoding.ASCII.GetBytes(_secret);

            services.AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(j =>
            {
                j.RequireHttpsMetadata = false;
                j.SaveToken = true;
                j.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<UserContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization();
        }
    }
}
