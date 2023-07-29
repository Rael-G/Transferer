using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Repositories
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly TransferoDbContext _context;

        public ArchiveRepository(TransferoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Archive>> GetAllAsync()
        {
            return await _context.Archives.ToListAsync();
        }

        public async Task<Archive?> GetByIdAsync(int id)
        {
            var file = await _context.Archives.FirstOrDefaultAsync(a => a.Id == id);
            return file;
        }

        public async Task<Archive> SaveAsync(Archive archive)
        {
            _context.Archives.Add(archive);
            await _context.SaveChangesAsync();

            return archive;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var archive = await GetByIdAsync(id);
            if (archive == null)
            {
                return false;
            }
            _context.Archives.Remove(archive);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
