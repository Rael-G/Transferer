namespace Api.Models.ViewModels
{
    public record TokenViewModel
    {
        public string AccessToken { get; set; }
        public DateTime Creation { get; set; }
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; }
    }
}
