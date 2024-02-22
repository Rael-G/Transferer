using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IArchiveRepository
    {
        /// <summary>
        /// Retrieve all archives from the database.
        /// </summary>
        /// <returns>Returns a list of all archives with associated user details.</returns>
        Task<List<Archive>> GetAllAsync();

        /// <summary>
        /// Retrieve archives associated with a specific user.
        /// </summary>
        /// <param name="Id">The ID of the user.</param>
        /// <returns>Returns a list of archives associated with the specified user.</returns>
        Task<List<Archive>> GetAllAsync(string Id);

        /// <summary>
        /// Retrieve a specific archive by its ID and associated user ID.
        /// </summary>
        /// <param name="id">The ID of the archive.</param>
        /// <param name="userId">The ID of the user associated with the archive.</param>
        /// <returns>Returns the archive with associated user details if found; otherwise, returns null.</returns>
        Task<Archive?> GetByIdAsync(Guid id, string userId);

        /// <summary>
        /// Retrieve archives by their IDs and associated user ID.
        /// </summary>
        /// <param name="ids">Array of archive IDs to retrieve.</param>
        /// <param name="userId">The ID of the user associated with the archives.</param>
        /// <returns>Returns a list of archives with associated user details.</returns>
        Task<List<Archive>> GetByIdsAsync(Guid[] ids, string userId);

        /// <summary>
        /// Search for archives by name and associated user ID.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <param name="userId">The ID of the user associated with the archives.</param>
        /// <returns>Returns a list of archives with associated user details matching the specified name.</returns>
        Task<List<Archive>?> GetByNameAsync(string name, string userId);

        /// <summary>
        /// Save a new archive to the database.
        /// </summary>
        /// <param name="archive">The archive to be saved.</param>
        /// <returns>Returns the saved archive with associated user details.</returns>
        Task<Archive> SaveAsync(Archive archive);

        /// <summary>
        /// Delete an archive from the database.
        /// </summary>
        /// <param name="archive">The archive to be deleted.</param>
        Task DeleteAsync(Archive archive);
    }
}
