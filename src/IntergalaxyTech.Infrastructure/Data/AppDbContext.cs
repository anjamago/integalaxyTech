using IntergalaxyTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntergalaxyTech.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Personaje> Personajes { get; set; } = null!;
    public DbSet<Solicitud> Solicitudes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Personaje>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Solicitud>()
            .HasKey(s => s.Id);
        
        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.PersonajeAsignado)
            .WithMany()
            .HasForeignKey(s => s.PersonajeId);
    }
}
