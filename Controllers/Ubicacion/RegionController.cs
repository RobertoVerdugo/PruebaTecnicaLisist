using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Ubicacion;
using System.Data;
using PruebaTecnicaLisit.Models.Application;
using Microsoft.AspNetCore.Identity;
using PruebaTecnicaLisit.Models.Logging;

namespace PruebaTecnicaLisit.Controllers.Ubicacion
{
    [Route("api/[controller]")]
	[Authorize(Roles = "Admin")]
	[ApiController]
	public class RegionController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly LoggerService _logger;
		private readonly UserManager<ApplicationUser> _userManager;
		public RegionController(ApplicationDbContext context, LoggerService logger, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_logger = logger;
			_userManager = userManager;
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<RegionDTOGet>>> GetRegions()
		{
			var regions = await _context.Regiones
				.Select(r => new RegionDTOGet
				{
					IdRegion = r.IdRegion,
					IdPais = r.IdPais,
					Nombre = r.Nombre,
					NombrePais = r.Pais.Nombre,
					NombreComunas = r.Comunas.Select(c => c.Nombre).ToList()
				})
				.ToListAsync();

			return regions;
		}
		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<ActionResult<RegionDTOGet>> GetRegion(int id)
		{
			var region = await _context.Regiones
				.Where(r => r.IdRegion == id)
				.Select(r => new RegionDTOGet
				{
					IdRegion = r.IdRegion,
					IdPais = r.IdPais,
					Nombre = r.Nombre,
					NombrePais = r.Pais.Nombre,
					NombreComunas = r.Comunas.Select(c => c.Nombre).ToList()
				})
				.FirstOrDefaultAsync();

			if (region == null)
				return NotFound("La región especificada no existe");

			return region;
		}

		[HttpPost]
		public async Task<ActionResult<Region>> PostRegion(RegionDTOPost regionDTO)
		{
			if (!ModelState.IsValid) 
				return BadRequest(ModelState);

			var pais = await _context.Paises
				.Include(p => p.Regiones)
				.FirstOrDefaultAsync(p => p.IdPais == regionDTO.IdPais);
			if (pais == null)
				return NotFound("El país especificado no existe");

			var region = new Region
			{
				Nombre = regionDTO.Nombre,
				IdPais = regionDTO.IdPais,
				Pais = pais
			};
			pais.Regiones.Add(region);

			_context.Regiones.Add(region);
			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), regionDTO.Nombre);
			return CreatedAtAction(nameof(GetRegion), new { id = region.IdRegion }, regionDTO);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutRegion(int id, RegionDTOPost regionDTO)
		{
			var region = await _context.Regiones
				.Include(r => r.Pais)
				.ThenInclude(p => p.Regiones)
				.FirstOrDefaultAsync(r => r.IdRegion == id);
			if (region == null)
				return NotFound("La región especificada no existe");

			var pais = await _context.Paises
				.Include(p => p.Regiones)
				.FirstOrDefaultAsync(p => p.IdPais == regionDTO.IdPais);
			if (pais == null)
				return NotFound("El país especificado no existe");

			region.Pais.Regiones.Remove(region);
			region.Nombre = regionDTO.Nombre;
			region.IdPais = regionDTO.IdPais;
			region.Pais = pais;
			pais.Regiones.Add(region);

			_context.Entry(region).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RegionExist(id))
					return NotFound();
				else
					throw;
			}
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteRegion(int id)
		{
			var region = await _context.Regiones
				.Include(r => r.Pais)
				.ThenInclude(p => p.Regiones)
				.FirstOrDefaultAsync(r => r.IdRegion == id);
			if (region == null)
				return NotFound("La región especificada no existe");

			region.Pais.Regiones.Remove(region);
			_context.Regiones.Remove(region);
			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
			return NoContent();
		}
		private bool RegionExist(int id)
		{
			return _context.Regiones.Any(e => e.IdRegion == id);
		}
	}
	public class RegionDTOPost
	{
		public int IdPais { get; set; }
		public string Nombre { get; set; }
	}
	public class RegionDTOGet
	{
		public int IdRegion { get; set; }
		public int IdPais { get; set; }
		public string Nombre { get; set; }
		public string NombrePais { get; set; }
		public List<string> NombreComunas { get; set; }
	}
}
