using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class User : IdentityUser
    {
        [JsonIgnore]
        public List<Archive> Archives { get; set; }

        public User() : base()
        {
            Archives = new List<Archive>();
        }

        public User(List<Archive> archives) : base() 
        { 
            Archives = archives;
        }
    }
}
