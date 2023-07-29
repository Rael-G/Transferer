using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Models;

namespace Api.Data.Repositories
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly TransferoDbContext _context;

        public ArchiveRepository(TransferoDbContext context) 
        {
            _context = context;
        }

        public async Task<Archive> GetByIdAsync(int id)
        {
            return _context.Archives.FirstOrDefault<Archive>(a => a.Id == id);
        }

        public async Task<Archive> SaveAsync(Archive archive)
        {
            _context.Archives.Add(archive);
            await _context.SaveChangesAsync();

            return archive;
        }
    }
}
