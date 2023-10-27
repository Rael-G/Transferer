using Api.Data.Interfaces;
using Api.Models;
using Api.Models.InputModel;
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

        public async Task<UserViewModel?> GetAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            var userViewModel = UserViewModel.MapToViewModel(user);

            return userViewModel;
        }

        public async Task<UserViewModel?> SearchAsync(string name)
        {
            var user = await _repository.GetByNameAsync(name);
            if (user == null)
            {
                return null; 
            }
            var userViewModel = UserViewModel.MapToViewModel(user);
            return userViewModel;
        }

        public async Task<UserViewModel?> EditAsync(UserInputModel userInputModel)
        {
            var user = await _repository.GetByIdAsync(userInputModel.Id);
            if (user == null)
            {
                return null;
            }
            UserInputModel.MapToModel(user, userInputModel);

            await _repository.UpdateAsync(user);

            return UserViewModel.MapToViewModel(user);
        }

        public async Task<UserViewModel?> RemoveAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            await _repository.DeleteAsync(user.Id);

            return UserViewModel.MapToViewModel(user);
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
