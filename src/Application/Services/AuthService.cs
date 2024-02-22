using Application.Dtos;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public AuthService(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<string?> CreateAsync(UserDto userDto, string password)
        {
            var user = _mapper.Map<User>(userDto);
            return await _repository.CreateAsync(user, password);
        }


        public async Task<TokenDto?> LoginAsync(UserDto userDto, string password)
        {
            var user = await _repository.GetByNameAsync(userDto.UserName);

            if (user is null)
                return null;

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, password);

            if (result != PasswordVerificationResult.Success)
                return null;

            var roles = await _repository.GetRolesAsync(user);
            var token = TokenService.GenerateToken(user, roles);

            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = token.Creation.AddDays(TokenService.DaysToExpiry);
            await _repository.UpdateAsync(user);

            return token;
        }

        public async Task<TokenDto?> RegenarateTokenAsync(string accessToken, string refreshToken)
        {
            var principal = TokenService.GetPrincipalFromToken(accessToken);

            var user = await _repository.GetByNameAsync(principal.Identity.Name);

            if (user == null ||
                user.RefreshToken != refreshToken ||
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
