using System.ComponentModel.DataAnnotations;

namespace Api.Models.InputModel
{
    public class TokenInputModel
    {
        [Required(AllowEmptyStrings = false)]
        public string AccessToken { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string RefreshToken { get; set; }
    }
}
