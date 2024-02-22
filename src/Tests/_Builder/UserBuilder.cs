using Api.Models;
using Application.Dtos;
using Bogus;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Tests._Builder
{

    public class UserBuilder
    {
        private string _id;
        private string _userName;
        private string _password;

        private readonly Faker _faker = new();

        public UserBuilder()
        {
            _id = new Guid().ToString();
            _userName = _faker.Name.FirstName();
            _password = _faker.Random.AlphaNumeric(10) + "!";
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

        public UserBuilder SetPassword(string password)
        {
            _password = password;
            return this;
        }

        public User Build()
        {
            User user = new User()
            {
                Id = _id,
                UserName = _userName
            };

            var passwordHasher = new PasswordHasher<User>();
            var hashedPassword = passwordHasher.HashPassword(user, _password);
            user.PasswordHash = hashedPassword;

            return user;
        }

        public UserDto BuildDto()
        {
            UserDto user = new UserDto()
            {
                Id = _id,
                UserName = _userName
            };

            var passwordHasher = new PasswordHasher<UserDto>();
            var hashedPassword = passwordHasher.HashPassword(user, _password);
            user.PasswordHash = hashedPassword;

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

        public static UserDto BuildUserDto()
        {

            return new UserBuilder().BuildDto();
        }

        public static List<UserDto> BuildUserDto(int num)
        {
            List<UserDto> users = new();
            for (int i = 0; i < num; i++)
            {
                users.Add(new UserBuilder().BuildDto());
            }

            return users;
        }

        public static (User user, UserDto userDto) BuildBoth()
        {
            var ub = new UserBuilder();

            return (ub.Build(), ub.BuildDto());
        }
    }
}
