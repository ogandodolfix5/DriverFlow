using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext   // ← Cambiado aquí
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tus tablas existentes
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<DriverProfile> DriverProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);   // ← Muy importante para Identity

            // Configuraciones adicionales
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasIndex(t => t.Fecha);
                entity.Property(t => t.MontoBruto).HasPrecision(18, 2);
                entity.Property(t => t.Gasolina).HasPrecision(18, 2);
                entity.Property(t => t.Mantenimiento).HasPrecision(18, 2);
                entity.Property(t => t.Comida).HasPrecision(18, 2);
                entity.Property(t => t.OtrosGastos).HasPrecision(18, 2);
                entity.Property(t => t.Tips).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Goal>(entity =>
            {
                entity.Property(g => g.MontoMeta).HasPrecision(18, 2);
            });
        }
    }
}