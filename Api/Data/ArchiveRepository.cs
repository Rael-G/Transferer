using Api.Models;

namespace Api.Data
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

        public Task<Archive> Save(int id)
        {
            throw new NotImplementedException();
        }
    }
}
