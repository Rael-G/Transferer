﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Authentication.Context;
using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Data.Repositories;

namespace Api
{
    public static class Settings
    {
        //InDevelopment
        public static readonly string _secret = "GUID SECRET";

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
                    $"Data Source={Directory.GetCurrentDirectory()}\\Auth.db;"
            ));

            var key = Encoding.ASCII.GetBytes(Settings._secret);

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
