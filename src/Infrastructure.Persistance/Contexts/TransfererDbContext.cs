using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Contexts
{
    public class TransfererDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public TransfererDbContext(DbContextOptions<TransfererDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Archive>(a =>
            {
                a.HasKey(a => a.Id);
                a.HasOne(a => a.User)
                .WithMany(u => u.Archives)
                .HasForeignKey(a => a.UserId);
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Archive> Archives { get; set; }
    }
}