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

        public Task<Archive> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Archive> SaveAsync(Archive archive)
        {
            _context.Archives.Add(archive);
            await _context.SaveChangesAsync();

            return archive;
        }
    }
}
