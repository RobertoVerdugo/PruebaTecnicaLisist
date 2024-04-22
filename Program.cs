using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PruebaTecnicaLisit
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			ConfigureServices(builder.Services, builder.Configuration);

			var app = builder.Build();

			Configure(app, app.Environment);

			//await InitializeRolesAsync(app.Services);

			app.Run();
		}

		private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddControllers();
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();

			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(
					configuration.GetConnectionString("DefaultConnection"),
					sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
			});

			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Password.RequiredLength = 8;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();
		}

		private static void Configure(WebApplication app, IHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();
		}
		private static async Task InitializeRolesAsync(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			// Crear roles si no existen
			var adminRoleExists = await roleManager.RoleExistsAsync("Administrador");
			if (!adminRoleExists)
			{
				await roleManager.CreateAsync(new IdentityRole("Administrador"));
			}

			var userRoleExists = await roleManager.RoleExistsAsync("UsuarioNormal");
			if (!userRoleExists)
			{
				await roleManager.CreateAsync(new IdentityRole("UsuarioNormal"));
			}
		}
	}
}
