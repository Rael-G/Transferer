namespace Application.Dtos
{
    public record TokenDto
    {
        public string AccessToken { get; set; }
        public DateTime Creation { get; set; }
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; }
    }
}
