using AppCoel.Core.Infra.Database.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace AppCoel.Core.Infra.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // Dbsets for your entities
        public DbSet<TbUser> Users { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
