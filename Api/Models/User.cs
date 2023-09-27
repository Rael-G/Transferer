using Microsoft.AspNetCore.Identity;

namespace Api.Models
{
    public class User : IdentityUser
    {
        public List<Archive>? Archives { get; set; }

        public User() { }

        public User(List<Archive> archives) : base() 
        { 
            Archives = archives;
        }
    }
}
