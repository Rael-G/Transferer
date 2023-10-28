using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public class TokenService
    {
        public static string GenerateToken(User user, List<string> userRoles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Settings._secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),

                Expires = DateTime.UtcNow.AddDays(1),

                Subject = GenerateClaims(user, userRoles)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static ClaimsIdentity GenerateClaims(User user, List<string> userRoles) 
        {
            var claims = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                });

            foreach(var role in userRoles)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}
