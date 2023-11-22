using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Api.Data.Repositories
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly TransfererDbContext _context;

        public ArchiveRepository(TransfererDbContext context)
        {
            _context = context;
        }

        public async Task<List<Archive>> GetAllAsync()
        {
            return await _context.Archives
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<List<Archive>> GetAllAsync(string Id)
        {
            return await _context.Archives
                .Where(a => a.UserId == Id)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<Archive?> GetByIdAsync(Guid id, string userId)
        {
            var archive = await _context.Archives
                .Where(a => a.Id == id && a.UserId == userId)
                .Include(a => a.User)
                .FirstOrDefaultAsync();

            return archive;
        }

        public async Task<List<Archive>> GetByIdsAsync(Guid[] ids, string userId)
        {
            List<Archive> archives = new();

            foreach (var id in ids)
            {
                var archive = await _context.Archives
                    .Where(a => a.Id == id && a.UserId == userId)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync();

                if (archive != null)
                {
                    archives.Add(archive);
                }
            }
            return archives;
        }

        public async Task<List<Archive>?> GetByNameAsync(string name, string userId)
        {
             var archives = await _context.Archives
                .Where(a => Regex.IsMatch(a.FileName.ToLower(), name.ToLower())
                    && a.UserId == userId)
                .Include(a => a.User)
                .ToListAsync();

            return archives;
        }

        public async Task<Archive> SaveAsync(Archive archive)
        {
            _context.Archives.Add(archive);
            await _context.SaveChangesAsync();

            return archive;
        }

        public async Task DeleteAsync(Archive archive)
        {
            _context.Archives.Remove(archive);
            await _context.SaveChangesAsync();
        }
    }
}
