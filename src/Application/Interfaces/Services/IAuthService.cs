using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Creates a new user asynchronously based on the provided login information.
        /// </summary>
        /// <param name="logInUser">Login information for the new user.</param>
        /// <returns>
        /// A string containing an error message if the creation fails; otherwise, returns null.
        /// </returns>
        Task<TokenDto?> LoginAsync(UserDto userDto, string password);

        /// <summary>
        /// Authenticates a user based on the provided login information.
        /// </summary>
        /// <param name="logInUser">Login information for authentication.</param>
        /// <returns>
        /// A LoggedUser object containing the user's username and authentication token if successful;
        /// otherwise, returns null.
        /// </returns>
        Task<string?> CreateAsync(UserDto userDto, string password);

        /// <summary>
        /// Regenerates an authentication token based on the provided expired access token.
        /// </summary>
        /// <param name="tokenInput">Token input model containing the expired access token and associated refresh token.</param>
        /// <returns>
        /// A new authentication token if regeneration is successful; otherwise, returns null.
        /// </returns>
        Task<TokenDto?> RegenarateTokenAsync(string accessToken, string refreshToken);
    }
}
