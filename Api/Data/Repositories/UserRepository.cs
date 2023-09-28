using Api.Data.Interfaces;
using Api.Models;

namespace Api.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        public List<User> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public User GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public User GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public User UpdateAsync(string id)
        {
            throw new NotImplementedException();
        }

        public User DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
