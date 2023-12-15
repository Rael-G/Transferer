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
        
        public async Task<string?> CreateAsync(LogInUser logInUser) =>
            await _repository.CreateAsync(logInUser);

        public async Task<TokenViewModel?> LoginAsync(LogInUser logInUser)
        {
            var user = await _repository.GetByNameAsync(logInUser.UserName);

            if (user is null)
                return null;
            

            // Create a PasswordHasher instance to verify the provided password against the stored hash.
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, logInUser.Password);
            
            TokenViewModel tokenViewModel = new();

            // If the password verification fails, return token view model with null properties.
            if (result != PasswordVerificationResult.Success)
                return tokenViewModel;

            // If the password verification is successful, generate an authentication token.
            var roles = await _repository.GetRolesAsync(user);
            var accessToken = TokenService.GenerateAccessToken(user, roles);
            var refreshToken = TokenService.GenerateRefreshToken();
            var now = DateTime.UtcNow;

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = now.AddDays(TokenService.DaysToExpiry);

            await _repository.UpdateAsync(user);

            tokenViewModel = new()
            {
                AccessToken = accessToken,
                Creation = now,
                Expiration = now.AddMinutes(TokenService.MinutesToExpiry),
                RefreshToken = refreshToken
            };

            return tokenViewModel;
        }
    }
}
