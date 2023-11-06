using Api.Models;
using System.Reflection;
using System.Security.Claims;

namespace Api.Business
{
    public interface IArchiveBusiness
    {
        Task<List<Archive>> GetAllAsync(string userId);
        Task<List<Archive>> GetAllAsync();
        Task<List<Archive>?> GetByNameAsync(string name, string userId);
        Task<Stream?> DownloadAsync(Archive archive);
        Task<(List<Archive> archives, string? missing)> GetByIdsAsync(Guid[] ids, string userId);
        Task<Archive?> GetByIdAsync(Guid id, string userId);
        Task<(byte[] data, string? missing)> DownloadMultipleAsync(List<Archive> archives);
        Task<List<Archive>> UploadAsync(IEnumerable<IFormFile> files, string userId);
        Task DeleteAsync(Archive archive);
        string GetUserIdFromClaims(ClaimsPrincipal user);
        Guid[]? ParseIds(string ids);
    }
}
