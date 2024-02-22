using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Dtos
{
    public class UserDto : IdentityUser
    {
        public List<Archive> Archives { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public UserDto() : base()
        {
            Archives = new List<Archive>();
        }

        public UserDto(List<Archive> archives) : base()
        {
            Archives = archives;
        }
    }
}
