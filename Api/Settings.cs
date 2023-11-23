using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Api.Data.Contexts;
using Api.Services;
using Microsoft.OpenApi.Models;
using Api.Models;
using Api.Business.Implementation;
using Api.Business;

namespace Api
{
    public static class Settings
    {
        //TODO: Make _secret secret
        public static readonly string _secret = "secret? 50a1b6e3-bfdb-448f-850f-17ff478f833d";

        public static void ConfigureAPI(this IServiceCollection services)
        {
            var dbDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Db");

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            services.AddDbContext<TransfererDbContext>(options =>
                options.UseSqlite($"Data Source={Path.Combine(dbDirectory, "Transferer.db")};"));
            
            var filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Files");

            if (!Directory.Exists(filesDirectory))
            {
                Directory.CreateDirectory(filesDirectory);
            }

            services.AddScoped<IFileStorage>(provider =>
                new LocalFileStorage(filesDirectory));

            services.AddScoped<IArchiveRepository, ArchiveRepository>();
            services.AddScoped<IUserBusiness, UserBusiness>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthBusiness, AuthBusiness>();
            services.AddScoped<IArchiveBusiness, ArchiveBusiness>();
        }

        public static void ConfigureAuth(this IServiceCollection services)
        {
            services.AddScoped<TransfererDbContext, TransfererDbContext>();

            var key = Encoding.ASCII.GetBytes(_secret);

            services.AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<TransfererDbContext>()
                .AddDefaultTokenProviders();
            services.AddAuthorization();

            SeedData(services);
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        private static void SeedData(IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var dbContext = serviceProvider.GetRequiredService<TransfererDbContext>();

            dbContext.Database.Migrate();
            Seeder.Seed(roleManager, userManager);
        }
    }
}
