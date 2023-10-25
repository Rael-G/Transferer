using Api.Models.ViewModels;
using System.Security.Claims;

namespace Api.Business.Contracts
{
    public interface IUserBusiness
    {
        Task<List<UserViewModel>> FindByNameAsync(string name);
        Task<UserViewModel> EditAsync(UserViewModel id);
        Task<UserViewModel> RemoveAsync(string id);
        string GetUserIdFromClaims(ClaimsPrincipal user);
        bool IsInRole(string role, ClaimsPrincipal user);
    }
}
