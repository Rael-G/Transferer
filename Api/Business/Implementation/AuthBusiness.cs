using Api.Data.Interfaces;
using Api.Models;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Api.Services;
using Microsoft.AspNetCore.Identity;

namespace Api.Business.Implementation
{
    public class AuthBusiness : IAuthBusiness
    {
        private readonly IUserRepository _repository;

        public AuthBusiness(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<string?> CreateAsync(LogInUser logInUser)
        {
            return await _repository.CreateAsync(logInUser);
        }

        public async Task<LoggedUser?> LoginAsync(LogInUser logInUser)
        {
            var user = await _repository.GetByNameAsync(logInUser.UserName);
            if (user == null)
            {
                return null;
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, logInUser.Password);
            string? token = null;

            if (result == PasswordVerificationResult.Success)
            {
                var roles = await _repository.GetRolesAsync(user);
                token = TokenService.GenerateToken(user, roles);
            }
            LoggedUser loggedUser = new() { UserName = user.UserName, Token = token };
            return loggedUser;
        }
    }
}
