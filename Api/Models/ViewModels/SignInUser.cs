using System.ComponentModel.DataAnnotations;

namespace Api.Models.ViewModels
{
    public record SignInUser
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public SignInUser(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
