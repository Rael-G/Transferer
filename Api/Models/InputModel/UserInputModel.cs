using Api.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.Models.InputModel
{
    public class UserInputModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        public UserInputModel(string id, string userName)
        {
            Id = id;
            UserName = userName;
        }

        public static User MapToModel(User user, UserInputModel inputModel)
        {
            user.UserName = inputModel.UserName;

            return user;
        }
    }
}
