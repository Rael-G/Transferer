using System.ComponentModel.DataAnnotations;

namespace Api.Models.InputModel
{
    public record LogInUser
    {
        [Required(AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public LogInUser()
        { }

        public LogInUser(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
