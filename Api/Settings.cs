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
        public static void ConfigureAPI(this IServiceCollection services, IConfiguration configuration)
        {
            TokenService.SecretKey = configuration["Secrets:SecretKey"] 
                ?? throw new ArgumentNullException("SecretKey", "Secret Key is not defined in appsettings.");

            services.AddCors(options => options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }));
            services.AddDbContext<TransfererDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
            
            var filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage");

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

            var key = Encoding.ASCII.GetBytes(TokenService.SecretKey);

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

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Transferer.Api",
                });

                var xmlFile = "Api.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });
        }

        public static void InitializeDb(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            Seeder.Seed(roleManager, userManager);
        }
    }
}
