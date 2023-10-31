using System.ComponentModel.DataAnnotations;

namespace Api.Models.InputModel
{
    public class UserInputModel
    {
        [Required(AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        public UserInputModel(string userName, string oldPassword, string newPassword)
        {
            UserName = userName;
            OldPassword = oldPassword;
            NewPassword = newPassword;
        }

        public static User MapToModel(User user, UserInputModel inputModel)
        {
            user.UserName = inputModel.UserName;
            return user;
        }
    }
}
