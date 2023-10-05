using Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Api.Extensions
{
    public static class UserManagerExtensions
    {
        public static string GetUserIdFromClaims(this UserManager<User> userManager, ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
        }
    }

}
