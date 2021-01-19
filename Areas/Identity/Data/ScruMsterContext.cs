using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScruMster.Areas.Identity.Data;

namespace ScruMster.Data
{
    public class ScruMsterContext : IdentityDbContext<ScruMsterUser>
    {
        public ScruMsterContext(DbContextOptions<ScruMsterContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
        public DbSet<ScruMsterUser> ScruMsterUsers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
    }
}
