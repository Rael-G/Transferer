using Api.Models;
using Api.Models.InputModel;
using System.Security.Claims;

namespace Api.Business
{
    public interface IUserBusiness
    {
        Task<User?> GetAsync(string id);
        Task<User?> SearchAsync(string name);
        Task<string?> EditAsync(User user, UserInputModel userInputModel);
        Task<User?> RemoveAsync(string id);
        string? GetUserIdFromClaims(ClaimsPrincipal user);
        bool IsInRole(string role, ClaimsPrincipal user);
    }
}
