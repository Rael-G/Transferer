﻿using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        public async Task<List<Archive>?> GetByIdsAsync(int[] ids)
        {
            var archives = await _context.Archives
                .Where(a => ids.Contains(a.Id ?? 0))
                .ToListAsync();

            return archives;
        }

        public async Task<List<Archive>?> GetByNameAsync(string name)
        {
             var archives = await _context.Archives
                .Where(a => Regex.IsMatch(a.FileName, name))
                .ToListAsync();

            return archives;
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
