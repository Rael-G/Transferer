using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistance.Services
{
    public class SeederService
    {
        public static void RoleSeeder(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("admin").Result)
            {
                var role = new IdentityRole { Name = "admin" };

                roleManager.CreateAsync(role).Wait();
            }
        }

        public static void UserSeeder(UserManager<User> userManager)
        {
            if (userManager.FindByNameAsync("admin").Result == null)
            {
                var user = new User { UserName = "admin" };
                var result = userManager.CreateAsync(user, "Aadmin1!").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "admin").Wait();
                }
            }
        }

        public static void Seed(RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager)
        {
            RoleSeeder(roleManager);
            UserSeeder(userManager);
        }

    }
}
