using System.ComponentModel.DataAnnotations;

namespace Authentication.ViewModel
{
    public record UserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        public UserViewModel(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
