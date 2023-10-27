using System.Text.Json.Serialization;

namespace Api.Models.ViewModels
{
    public record class UserViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public static UserViewModel MapToViewModel(User user)
        {
            var viewModel = new UserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName
            };
            return viewModel;
        }

        public static List<UserViewModel> MapToViewModel(IEnumerable<User> users)
        {
            List<UserViewModel> viewModels = new();
            foreach (var user in users)
            {
                viewModels.Add(MapToViewModel(user));
            }

            return viewModels;
        }
    }
}
