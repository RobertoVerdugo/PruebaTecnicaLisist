using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Ubicacion;
using PruebaTecnicaLisit.Models.ServiciosSociales;
using PruebaTecnicaLisit.Models.Logging;

namespace PruebaTecnicaLisit.Models.Application
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Pais> Paises { get; set; }
        public DbSet<Region> Regiones { get; set; }
        public DbSet<Comuna> Comunas { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<AsignacionServicio> ServiciosUsuario { get; set; }
		public DbSet<Logger> Logs { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Region>()
                .HasOne(r => r.Pais)
                .WithMany(p => p.Regiones)
                .HasForeignKey(r => r.IdPais)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comuna>()
                .HasOne(c => c.Region)
                .WithMany(r => r.Comunas)
                .HasForeignKey(c => c.IdRegion)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Comuna)
                .WithMany()
                .HasForeignKey(u => u.IdComuna);

            modelBuilder.Entity<AsignacionServicio>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.ServiciosUsuario)
                .HasForeignKey(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AsignacionServicio>()
                .HasOne(a => a.Servicio)
                .WithMany(s => s.ServiciosUsuario)
                .HasForeignKey(a => a.IdServicio)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Servicio>()
            .HasMany(s => s.Comunas)
            .WithMany(c => c.Servicios)
            .UsingEntity(j => j.ToTable("ServiciosComuna"));

        }

    }
}
