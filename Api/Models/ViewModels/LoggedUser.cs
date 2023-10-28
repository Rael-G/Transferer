namespace Api.Models.ViewModels
{
    public record LoggedUser
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
