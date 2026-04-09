using Microsoft.EntityFrameworkCore;
using SYSGES_MAGs.Models;

namespace SYSGES_MAGs.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructeur EF Core
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; } 
        public DbSet<Profil> Profiles { get; set; }
        public DbSet<BkPrdCli> bkprdclis { get; set; }
        public DbSet<Bkmvti> Bkmvtis { get; set; }
        public DbSet<TypeMag> TypeMags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BkPrdCli>()
                .ToTable("bkprdcli") 
                .HasNoKey(); // Si pas de clé primaire
        }
    }
}
