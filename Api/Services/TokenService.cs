using Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public static class TokenService
    {
        public const int MinutesToExpiry = 30;

        public const int DaysToExpiry = 7;

        /// <summary>
        /// Generates a JWT token for the specified user and user roles.
        /// </summary>
        /// <param name="user">The user for whom the token is generated.</param>
        /// <param name="userRoles">The roles associated with the user.</param>
        /// <returns>The generated JWT token as a string.</returns>
        public static string GenerateAccessToken(User user, List<string> userRoles)
        {
            var claimsIdentity = GenerateClaimsIdentity(user, userRoles);

            return GenerateAccessToken(claimsIdentity);
        }

        /// <summary>
        /// Generates a JWT token based on the provided claims.
        /// </summary>
        /// <param name="claims">The claims to include in the token.</param>
        /// <returns>The generated JWT token as a string.</returns>
        public static string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var claimsIdentity = GenerateClaimsIdentity(claims);

            return GenerateAccessToken(claimsIdentity);
        }

        /// <summary>
        /// Generates a refresh token using a new Guid.
        /// </summary>
        /// <returns>The generated refresh token as a string.</returns>
        public static string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Retrieves a ClaimsPrincipal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The ClaimsPrincipal extracted from the token.</returns>
        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(Settings.SecretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals
                (SecurityAlgorithms.HmacSha256, StringComparison.InvariantCulture))
                throw new SecurityTokenException("Invalid Token.");
            
            return principal;
        }

        /// <summary>
        /// Generates a JWT token based on the provided claims identity.
        /// </summary>
        /// <param name="claimsIdentity">The claims identity for the token.</param>
        /// <returns>The generated JWT token as a string.</returns>
        private static string GenerateAccessToken(ClaimsIdentity claimsIdentity)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Settings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),

                Expires = DateTime.UtcNow.AddMinutes(MinutesToExpiry),

                Subject = claimsIdentity
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates the claims for the JWT token, including user-specific and role claims.
        /// </summary>
        /// <param name="user">The user for whom the claims are generated.</param>
        /// <param name="userRoles">The roles associated with the user.</param>
        /// <returns>A ClaimsIdentity containing user-specific and role claims.</returns>
        private static ClaimsIdentity GenerateClaimsIdentity(User user, List<string> userRoles) 
        {
            var claimsIdentity = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                });

            foreach(var role in userRoles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return claimsIdentity;
        }

        /// <summary>
        /// Generates a ClaimsIdentity based on the provided claims.
        /// </summary>
        /// <param name="claims">The claims to include in the identity.</param>
        /// <returns>A ClaimsIdentity containing the provided claims.</returns>
        private static ClaimsIdentity GenerateClaimsIdentity(IEnumerable<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims);

            return claimsIdentity;
        }
    }
}
