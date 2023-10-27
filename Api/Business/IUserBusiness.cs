using Api.Models.InputModel;
using Api.Models.ViewModels;
using System.Security.Claims;

namespace Api.Business
{
    public interface IUserBusiness
    {
        Task<UserViewModel?> GetAsync(string id);
        Task<UserViewModel?> SearchAsync(string name);
        Task<UserViewModel?> EditAsync(UserInputModel user);
        Task<UserViewModel?> RemoveAsync(string id);
        string? GetUserIdFromClaims(ClaimsPrincipal user);
        bool IsInRole(string role, ClaimsPrincipal user);
    }
}
