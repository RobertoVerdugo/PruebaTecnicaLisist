using Microsoft.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Application;

namespace PruebaTecnicaLisit.Models
{
	public static class MigrationExtensions
	{
		public static void ApplyMigrations(this IApplicationBuilder app)
		{
			using IServiceScope scope = app.ApplicationServices.CreateScope();
			using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			dbContext.Database.Migrate();
		}
	}
}
