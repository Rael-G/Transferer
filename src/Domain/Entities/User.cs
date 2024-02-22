using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser
    {
        public List<Archive> Archives { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public User() : base()
        {
            Archives = [];
        }

        public User(IEnumerable<Archive> archives) : base()
        {
            Archives = archives.ToList();
        }
    }
}
