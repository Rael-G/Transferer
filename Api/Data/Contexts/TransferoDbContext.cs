using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Contexts
{
    public class TransferoDbContext : DbContext
    {
        public TransferoDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Archive> Archives { get; set; }
    }
}
