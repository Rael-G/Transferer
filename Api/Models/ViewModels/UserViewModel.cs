namespace Api.Models.ViewModels
{
    public record class UserViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public UserViewModel (User user)
        {
            Id = user.Id;
            UserName = user.Id.ToString();
        }

        public static List<UserViewModel> MapUsersToViewModel(IEnumerable<User> users)
        {
            List<UserViewModel> viewModels = new();
            foreach (var user in users)
            {
                viewModels.Add(new UserViewModel(user));
            }

            return viewModels;
        }

        public static User MapToUser(User user, UserViewModel userViewModel)
        {
            user.UserName = userViewModel.UserName;

            return user;
        }
    }
}
