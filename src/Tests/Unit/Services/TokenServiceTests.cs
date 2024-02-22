using Api.Models;
using Api.Services;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Tests.Unit.Services
{
    public class TokenServiceTests
    {
        private readonly string TestSecretKey = Guid.NewGuid().ToString();

        public TokenServiceTests()
        {
            TokenService.SecretKey = TestSecretKey;
        }

        [Fact]
        public void GenerateToken_ValidUserAndRoles_ReturnsToken()
        {
            var user = new User { Id = "1", UserName = "testuser" };
            var userRoles = new List<string> { "Role1", "Role2" };

            var result = TokenService.GenerateToken(user, userRoles);

            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.True(result.Creation != default);
            Assert.True(result.Expiration != default);
        }

        [Fact]
        public void GenerateToken_Claims_ReturnsToken()
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", "1"),
                new Claim(ClaimTypes.Name, "testuser"),
            };

            var result = TokenService.GenerateToken(claims);

            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.True(result.Creation != default);
            Assert.True(result.Expiration != default);
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ValidToken_ReturnsClaimsPrincipal()
        {
            var user = new User { Id = "1", UserName = "testuser" };
            var userRoles = new List<string> { "Role1", "Role2" };
            var validToken = TokenService.GenerateToken(user, userRoles);

            var result = TokenService.GetPrincipalFromToken(validToken.AccessToken);

            Assert.NotNull(result);
            Assert.IsType<ClaimsPrincipal>(result);
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_InvalidToken_ThrowsSecurityTokenException()
        {
            var invalidToken = "invalid_token";

            Assert.Throws<SecurityTokenMalformedException>(() => TokenService.GetPrincipalFromToken(invalidToken));
        }
    }
}
