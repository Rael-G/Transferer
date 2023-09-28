using Api.Models;

namespace Api.Data.Interfaces
{
    public interface IUserRepository
    {
        List<User> GetAllAsync();
        User GetByIdAsync(string id);
        User GetByNameAsync(string name);
        User UpdateAsync(string id);
        User DeleteAsync(string id);
    }
}
