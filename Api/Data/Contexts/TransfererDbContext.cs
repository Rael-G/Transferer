using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Contexts
{
    public class TransfererDbContext : DbContext
    {
        public TransfererDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Archive> Archives { get; set; }
    }
}
