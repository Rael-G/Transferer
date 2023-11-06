using Api.Models;
using Api.Models.InputModel;

namespace Api.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByNameAsync(string name);
        Task UpdateAsync(User user);
        Task<string?> UpdateAsync(User user, UserInputModel inputModel);
        Task DeleteAsync(string id);
        Task<string?> CreateAsync(LogInUser signInUser);
        Task<List<string>> GetRolesAsync(User user);
    }
}
