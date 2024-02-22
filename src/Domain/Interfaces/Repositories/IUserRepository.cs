using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>Returns the user if found; otherwise, returns null.</returns>
        Task<User?> GetByIdAsync(string id);

        /// <summary>
        /// Retrieves a user by their username asynchronously.
        /// </summary>
        /// <param name="name">The username of the user to retrieve.</param>
        /// <returns>Returns the user if found; otherwise, returns null.</returns>
        Task<User?> GetByNameAsync(string name);

        /// <summary>
        /// Updates a user asynchronously.
        /// </summary>
        /// <param name="user">The user to be updated.</param>
        Task UpdateAsync(User user);

        /// <summary>
        /// Updates a user's password and username asynchronously.
        /// </summary>
        /// <param name="user">The user to be updated.</param>
        /// <param name="inputModel">The input model containing new password and username.</param>
        /// <returns>
        /// Returns an error message if the password or username update fails; otherwise, returns null.
        /// </returns>
        Task<IdentityResult> UpdateAsync(User user, string oldPassword, string newPassword);

        /// <summary>
        /// Deletes a user by their ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        Task DeleteAsync(string id);

        /// <summary>
        /// Creates a new user asynchronously using the provided sign-in user details.
        /// </summary>
        /// <param name="signInUser">The sign-in user details.</param>
        /// <returns>
        /// Returns an error message if user creation fails; otherwise, returns null indicating success.
        /// </returns>
        Task<string?> CreateAsync(User user, string password);

        /// <summary>
        /// Retrieves the roles associated with a user asynchronously.
        /// </summary>
        /// <param name="user">The user for whom roles are to be retrieved.</param>
        /// <returns>Returns a list of roles associated with the user.</returns>
        Task<List<string>> GetRolesAsync(User user);
    }
}
