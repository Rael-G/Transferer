namespace Api.Models.ViewModels
{
    public record LogedUser
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
