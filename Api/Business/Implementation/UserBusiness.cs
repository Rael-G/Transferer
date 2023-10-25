using Api.Business.Contracts;
using Api.Models;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Api.Business.Implementation
{
    public class UserBusiness : IUserBusiness
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserBusiness(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserViewModel>> FindByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<UserViewModel> EditAsync(UserViewModel id)
        {
            throw new NotImplementedException();
        }

        public Task<UserViewModel> RemoveAsync(string id)
        {
            throw new NotImplementedException();
        }

        public string GetUserIdFromClaims(ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
        }

        public bool IsInRole(string role, ClaimsPrincipal user)
        {
            return user.IsInRole(role);
        }
    }
}
