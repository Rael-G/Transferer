using Api.Models;

namespace Api.Data
{
    public interface IArchiveRepository
    {
        Task<Archive> Save(int id);
        Task<Archive> GetById(int id);
    }
}
