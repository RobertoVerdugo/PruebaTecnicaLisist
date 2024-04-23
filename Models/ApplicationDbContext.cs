using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Ubicacion;
using PruebaTecnicaLisit.Models.ServiciosSociales;

namespace PruebaTecnicaLisit.Models
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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Region>()
                .HasOne(r => r.Pais)
                .WithMany(p => p.Regiones)
                .HasForeignKey(r => r.IdPais);

            modelBuilder.Entity<Comuna>()
                .HasOne(c => c.Region)
                .WithMany(r => r.Comunas)
                .HasForeignKey(c => c.IdRegion);

			modelBuilder.Entity<ApplicationUser>()
				.HasOne(u => u.Comuna)
				.WithMany()
				.HasForeignKey(u => u.IdComuna);

			modelBuilder.Entity<AsignacionServicio>()
				.HasOne(a => a.Usuario)
				.WithMany(u => u.ServiciosUsuario)
				.HasForeignKey(a => a.IdUsuario);

			modelBuilder.Entity<AsignacionServicio>()
				.HasOne(a => a.Servicio)
				.WithMany(s => s.ServiciosUsuario)
				.HasForeignKey(a => a.IdServicio);

			modelBuilder.Entity<Servicio>()
			.HasMany(s => s.Comunas)
			.WithMany(c => c.Servicios)
			.UsingEntity(j => j.ToTable("ServiciosComuna"));

		}

    }
}
