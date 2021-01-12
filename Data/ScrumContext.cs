using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScruMster.Models;
using Microsoft.EntityFrameworkCore;

namespace ScruMster.Data
{
    public class ScrumContext : DbContext
    {
        public ScrumContext(DbContextOptions<ScrumContext> options) : base(options)
        {
        }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Sprint> Sprints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>().ToTable("Teams");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Sprint>().ToTable("Sprints");
        }
    }
}