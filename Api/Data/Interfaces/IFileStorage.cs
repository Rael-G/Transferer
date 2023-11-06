namespace Api.Data.Interfaces
{
    public interface IFileStorage
    {
        //Store a file and return its path or url
        Task<string> StoreAsync(Stream file);
        Task<Stream?> GetByPathAsync(string path);
        Task<bool> DeleteAsync(string path);
    }
}
