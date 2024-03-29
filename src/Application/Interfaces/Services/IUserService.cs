﻿using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Get a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>Returns the user if found; otherwise, returns null.</returns>
        Task<UserDto?> GetAsync(string id);

        /// <summary>
        /// Search for a user by name.
        /// </summary>
        /// <param name="name">The name of the user to search for.</param>
        /// <returns>Returns the user if found; otherwise, returns null.</returns>
        Task<UserDto?> SearchAsync(string name);

        /// <summary>
        /// Edit the details of a user.
        /// </summary>
        /// <param name="user">The user to edit.</param>
        /// <param name="userInputModel">The input model containing the updated user details.</param>
        /// <returns>Returns an error message if the edit is unsuccessful; otherwise, returns null.</returns>
        Task<IdentityResult> EditAsync(UserDto userDto, string oldPassword, string newPassword);

        /// <summary>
        /// Remove a user and delete all associated archives.
        /// </summary>
        /// <param name="id">The ID of the user to remove.</param>
        /// <returns>Returns the removed user if found; otherwise, returns null.</returns>
        Task<UserDto?> RemoveAsync(string id);

        /// <summary>
        /// Get the user ID from the claims of a user.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <returns>Returns the user ID extracted from the claims; otherwise, returns null.</returns>
        string? GetUserIdFromClaims(ClaimsPrincipal user);

        /// <summary>
        /// Check if a user is in a specific role.
        /// </summary>
        /// <param name="role">The role to check.</param>
        /// <param name="user">The claims principal representing the user.</param>
        /// <returns>Returns true if the user is in the specified role; otherwise, returns false.</returns>
        bool IsInRole(string role, ClaimsPrincipal user);
    }
}
