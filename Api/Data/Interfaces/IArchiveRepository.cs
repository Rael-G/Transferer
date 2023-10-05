using Api.Models;

namespace Api.Data.Interfaces
{
    public interface IArchiveRepository
    {
        Task<List<Archive>> GetAllAsync();
        Task<List<Archive>> GetAllAsync(string Id);
        Task<Archive?> GetByIdAsync(Guid id, string userId);
        Task<List<Archive>> GetByIdsAsync(Guid[] ids, string userId);
        Task<List<Archive>?> GetByNameAsync(string name, string userId);
        Task<Archive> SaveAsync(Archive archive);
        Task DeleteAsync(Guid id, string userId);
    }
}
