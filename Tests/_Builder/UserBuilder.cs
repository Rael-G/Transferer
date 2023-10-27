using Api.Models;
using Api.Models.ViewModels;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests._Builder
{

    public class UserBuilder
    {
        private string _id;
        private string _userName;

        private readonly Faker _faker = new();

        public UserBuilder()
        {
            _id = new Guid().ToString();
            _userName = _faker.Name.FirstName();
        }

        public UserBuilder SetId(string id)
        {
            _id = id;
            return this;
        }

        public UserBuilder SetName(string name)
        {
            _userName = name;
            return this;
        }

        public User Build()
        {
            User user = new User()
            {
                Id = _id,
                UserName = _userName
            };
            return user;
        }

        public static User BuildUser()
        {

            return new UserBuilder().Build(); 
        }

        public static List<User> BuildUser(int num)
        {
            List<User> users = new();
            for (int i = 0; i < num; i++)
            {
                users.Add(new UserBuilder().Build());
            }

            return users;
        }
    }
}
