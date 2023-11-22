using Api.Data.Interfaces;
using Api.Models;
using Api.Models.InputModel;
using System.Security.Claims;

namespace Api.Business.Implementation
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IUserRepository _repository;
        private readonly IArchiveBusiness _archiveBusiness;

        public UserBusiness(IUserRepository repository, IArchiveBusiness archiveBusiness)
        {
            _repository = repository;
            _archiveBusiness = archiveBusiness;
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
                return null;
            
            foreach (var archive in user.Archives)
                await _archiveBusiness.DeleteAsync(archive);
            
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
