using Api.Data.Interfaces;
using Api.Interfaces.Services;
using Api.Models;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;

        public AuthService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<string?> CreateAsync(LogInUser logInUser) =>
            await _repository.CreateAsync(logInUser);

        public async Task<Token?> LoginAsync(LogInUser logInUser)
        {
            var user = await _repository.GetByNameAsync(logInUser.UserName);

            if (user is null)
                return null;

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, logInUser.Password);

            if (result != PasswordVerificationResult.Success)
                return null;

            var roles = await _repository.GetRolesAsync(user);
            var token = TokenService.GenerateToken(user, roles);

            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = token.Creation.AddDays(TokenService.DaysToExpiry);
            await _repository.UpdateAsync(user);

            return token;
        }

        public async Task<Token?> RegenarateTokenAsync(TokenInputModel tokenInput)
        {
            var principal = TokenService.GetPrincipalFromToken(tokenInput.AccessToken);

            var user = await _repository.GetByNameAsync(principal.Identity.Name);

            if (user == null ||
                user.RefreshToken != tokenInput.RefreshToken ||
                user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return null;

            var token = TokenService.GenerateToken(principal.Claims);

            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = token.Creation.AddDays(TokenService.DaysToExpiry);

            await _repository.UpdateAsync(user);

            return token;
        }
    }
}
