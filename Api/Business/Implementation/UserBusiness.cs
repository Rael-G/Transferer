using Api.Business.Contracts;
using Api.Data.Interfaces;
using Api.Models;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Api.Business.Implementation
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IUserRepository _repository;

        public UserBusiness(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserViewModel>> SearchAsync(string name)
        {
            var users = await _repository.GetByNameAsync(name);
            var usersViewModel = UserViewModel.MapUsersToViewModel(users);
            return usersViewModel;
        }

        public async Task<UserViewModel?> EditAsync(UserViewModel userViewModel)
        {
            var user = await _repository.GetByIdAsync(userViewModel.Id);
            if (user == null)
            {
                return null;
            }
            UserViewModel.MapToUser(user, userViewModel);

            await _repository.UpdateAsync(user);

            return new UserViewModel(user);
        }

        public async Task<UserViewModel?> RemoveAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            await _repository.DeleteAsync(user.Id);

            return new UserViewModel(user);
        }

        public string? GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (claim != null)
            {
                return claim.Value;
            }

            return null;
        }

        public bool IsInRole(string role, ClaimsPrincipal user)
        {
            return user.IsInRole(role);
        }
    }
}
