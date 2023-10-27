using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Api.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
           return await _userManager.FindByIdAsync(id);
        }

        public async Task<User?> GetByNameAsync(string name)
        {
            return await _userManager.FindByNameAsync(name);
        }

        public async Task UpdateAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task DeleteAsync(string id)
        {
            var user = await GetByIdAsync(id);
            await _userManager.DeleteAsync(user);
        }
    }
}
