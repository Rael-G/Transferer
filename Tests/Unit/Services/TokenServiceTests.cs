using Api.Models;
using Api.Services;
using System.Security.Claims;

namespace Tests.Unit.Services
{
    public class TokenServiceTests
    {
        [Fact]
        public void GenerateAccessToken_ValidUserAndRoles_ReturnsToken()
        {
            //SecretKey is empty without this
            TokenService.SecretKey = Guid.NewGuid().ToString();

            var user = new User { Id = "1", UserName = "testuser" };
            var userRoles = new List<string> { "Role1", "Role2" };

            var result = TokenService.GenerateAccessToken(user, userRoles);

            Assert.NotNull(result);
        }

        [Fact]
        public void GenerateAccessToken_Claims_ReturnsToken()
        {
            TokenService.SecretKey = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim("UserId", "1"),
                new Claim(ClaimTypes.Name, "testuser"),
            };

            var result = TokenService.GenerateAccessToken(claims);

            Assert.NotNull(result);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsToken()
        {
            var result = TokenService.GenerateRefreshToken();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ValidToken_ReturnsClaimsPrincipal()
        {
            TokenService.SecretKey = Guid.NewGuid().ToString();

            var validToken = TokenService.GenerateAccessToken(new List<Claim>());

            var result = TokenService.GetPrincipalFromExpiredToken(validToken);

            Assert.NotNull(result);
        }
    }
}
