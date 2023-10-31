using Api.Data.Interfaces;
using Api.Models;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Api.Services;
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

        public async Task<User?> GetAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task<User?> SearchAsync(string name)
        {
            var user = await _repository.GetByNameAsync(name);
            return user;
        }

        public async Task<string?> EditAsync(User user, UserInputModel userInputModel)
        {
            
            var result = await _repository.UpdateAsync(user, userInputModel);
            return result;
        }

        public async Task<User?> RemoveAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            await _repository.DeleteAsync(user.Id);

            return user;
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
