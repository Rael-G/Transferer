﻿using Api.Models;
using System.Reflection;
using System.Security.Claims;

namespace Api.Business
{
    public interface IArchiveBusiness
    {
        /// <summary>
        /// Gets all archives for a specific user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of archives belonging to the specified user.</returns>
        Task<List<Archive>> GetAllAsync(string userId);

        /// <summary>
        /// Gets all archives asynchronously.
        /// </summary>
        /// <returns>A list of all archives in the system.</returns>
        Task<List<Archive>> GetAllAsync();

        /// <summary>
        /// Gets archives by name and the associated user asynchronously.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of archives matching the specified name and user.</returns>
        Task<List<Archive>?> GetByNameAsync(string name, string userId);

        /// <summary>
        /// Downloads the content of an archive asynchronously.
        /// </summary>
        /// <param name="archive">The archive to download.</param>
        /// <returns>The stream containing the archive content.</returns>
        Task<Stream?> DownloadAsync(Archive archive);

        /// <summary>
        /// Gets archives by their IDs and the associated user asynchronously.
        /// </summary>
        /// <param name="ids">Array of archive IDs.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A tuple containing a list of archives and a string listing missing archives.</returns>
        Task<(List<Archive> archives, string? missing)> GetByIdsAsync(Guid[] ids, string userId);
        
        /// <summary>
        /// Gets an archive by its ID and the associated user asynchronously.
        /// </summary>
        /// <param name="id">The ID of the archive.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The archive associated with the specified ID and user.</returns>
        Task<Archive?> GetByIdAsync(Guid id, string userId);

        /// <summary>
        /// Downloads the content of multiple archives as a zip file asynchronously.
        /// </summary>
        /// <param name="archives">List of archives to include in the zip file.</param>
        /// <returns>
        /// A tuple containing the byte array of the zip file data and a string listing missing archives.
        /// </returns>
        Task<(byte[] data, string? missing)> DownloadMultipleAsync(List<Archive> archives);

        /// <summary>
        /// Uploads one or more files for a specific user asynchronously.
        /// </summary>
        /// <param name="files">The files to upload.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of Archive objects representing the uploaded files.</returns>
        Task<List<Archive>> UploadAsync(IEnumerable<IFormFile> files, string userId);

        /// <summary>
        /// Deletes an archive, removes it from the associated user, and deletes the file from storage.
        /// </summary>
        /// <param name="archive">The archive to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(Archive archive);

        /// <summary>
        /// Gets the user ID from the claims of a user.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <returns>The user ID extracted from the claims.</returns>
        string GetUserIdFromClaims(ClaimsPrincipal user);
    }
}
