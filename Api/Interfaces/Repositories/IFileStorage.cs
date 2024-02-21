namespace Api.Interfaces.Repositories
{
    public interface IFileStorage
    {
        /// <summary>
        /// Stores a file stream asynchronously and returns the path of the stored file.
        /// </summary>
        /// <param name="file">The stream containing the file content to be stored.</param>
        /// <returns>Returns the path of the stored file.</returns>
        Task<string> StoreAsync(Stream file);

        /// <summary>
        /// Retrieves a file stream from the specified path asynchronously.
        /// </summary>
        /// <param name="path">The path of the file to retrieve.</param>
        /// <returns>
        /// Returns a stream containing the file content if the file exists; otherwise, returns null.
        /// </returns>
        Task<Stream?> GetByPathAsync(string path);

        /// <summary>
        /// Deletes a file specified by its path asynchronously.
        /// </summary>
        /// <param name="path">The path of the file to be deleted.</param>
        /// <returns>Returns a boolean indicating whether the file was successfully deleted.</returns>
        Task<bool> DeleteAsync(string path);
    }
}
