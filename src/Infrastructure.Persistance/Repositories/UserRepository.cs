using Domain.Entities;
using Domain.Interfaces.Repositories;
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

        public async Task<IdentityResult> UpdateAsync(User user, string oldPassword, string newPassword)
        {
            var passwordResult = await _userManager
                .ChangePasswordAsync(user, oldPassword, newPassword);

            if (!passwordResult.Succeeded)
            {
                return passwordResult;
            }

            return await _userManager.UpdateAsync(user);
        }

        public async Task DeleteAsync(string id)
        {
            var user = await GetByIdAsync(id);
            await _userManager.DeleteAsync(user);
        }

        public async Task<string?> CreateAsync(User user, string password)
        {

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                return null;
            }
            return result.ToString();
        }

        public async Task<List<string>> GetRolesAsync(User user)
        {
            var result = await _userManager.GetRolesAsync(user);

            return result.ToList();
        }
    }
}
