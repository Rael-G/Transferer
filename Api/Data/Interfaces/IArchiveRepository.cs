using Api.Models;

namespace Api.Data.Interfaces
{
    public interface IArchiveRepository
    {
        Task<List<Archive>> GetAllAsync();
        Task<Archive?> GetByIdAsync(int id);
        Task<(List<Archive>?, List<int>)> GetByIdsAsync(int[] ids);
        Task<List<Archive>?> GetByNameAsync(string name);
        Task<Archive> SaveAsync(Archive archive);
        Task<bool> DeleteAsync(int id);
    }
}
