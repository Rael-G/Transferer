using Api.Models;
using System.Security.Claims;

namespace Api.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByNameAsync(string name);
        Task UpdateAsync(User user);
        Task DeleteAsync(string id);
    }
}
