using Api.Data.Interfaces;
using Api.Models;
using Api.Models.InputModel;
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

        public async Task<string?> UpdateAsync(User user, UserInputModel inputModel)
        {
            var passwordResult = await _userManager
                .ChangePasswordAsync(user, inputModel.OldPassword, inputModel.NewPassword);

            if (!passwordResult.Succeeded)
            {
                return passwordResult.ToString();
            }

            var nameResult = await _userManager.SetUserNameAsync(user, inputModel.UserName);

            if (!nameResult.Succeeded)
            {
                return nameResult.ToString();
            }

            return null;
        }

        public async Task DeleteAsync(string id)
        {
            var user = await GetByIdAsync(id);
            await _userManager.DeleteAsync(user);
        }

        public async Task<string?> CreateAsync(LogInUser signInUser)
        {
            var user = new User() { UserName = signInUser.UserName};

            var result = await _userManager.CreateAsync(user, signInUser.Password);

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
