using Api.Models.InputModel;
using Api.Models.ViewModels;

namespace Api.Business
{
    public interface IAuthBusiness
    {
        /// <summary>
        /// Creates a new user asynchronously based on the provided login information.
        /// </summary>
        /// <param name="logInUser">Login information for the new user.</param>
        /// <returns>
        /// A string containing an error message if the creation fails; otherwise, returns null.
        /// </returns>
        Task<LoggedUser?> LoginAsync(LogInUser logInUser);

        /// <summary>
        /// Authenticates a user based on the provided login information.
        /// </summary>
        /// <param name="logInUser">Login information for authentication.</param>
        /// <returns>
        /// A LoggedUser object containing the user's username and authentication token if successful;
        /// otherwise, returns null.
        /// </returns>
        Task<string?> CreateAsync(LogInUser logInUser);
    }
}
