using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Ubicacion;
using PruebaTecnicaLisit.Models;
using System.Data;
using System.Runtime.InteropServices;
using PruebaTecnicaLisit.Models.ServiciosSociales;

namespace PruebaTecnicaLisit.Controllers.Ubicacion
{
	[Route("api/[controller]")]
	//[Authorize(Roles = "Admin")]
	[ApiController]
	public class ComunaController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public ComunaController(ApplicationDbContext context)
		{
			_context = context;
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ComunaDTOGet>>> GetComunas()
		{
			var comunas = await _context.Comunas
				.Select(c => new ComunaDTOGet
				{
					IdComuna = c.IdComuna,
					IdRegion = c.IdRegion,
					Nombre = c.Nombre,
					NombreRegion = c.Region.Nombre,
					NombreServicios = c.Servicios.Select(s => s.Nombre).ToList()
				})
				.ToListAsync();

			return comunas;
		}

		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<ActionResult<ComunaDTOGet>> GetComuna(int id)
		{
			var comuna = await _context.Comunas
				.Where(c => c.IdComuna == id)
				.Select(c => new ComunaDTOGet
				{
					IdComuna = c.IdComuna,
					IdRegion = c.IdRegion,
					Nombre = c.Nombre,
					NombreRegion = c.Region.Nombre,
					NombreServicios = c.Servicios.Select(s => s.Nombre).ToList()
				})
				.FirstOrDefaultAsync();

			if (comuna == null)
				return NotFound();

			return comuna;
		}

		[HttpPost]
		public async Task<ActionResult<Comuna>> PostComuna(ComunaDTO comunaDTO)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var region = await _context.Regiones
				.Include(r => r.Comunas)
				.FirstOrDefaultAsync(r => r.IdRegion == comunaDTO.IdRegion);


			if (region == null)
				return NotFound("La region especificada no existe");
			var comuna = new Comuna
			{
				Nombre = comunaDTO.Nombre,
				IdRegion = comunaDTO.IdRegion,
				Region = region
			};
			region.Comunas.Add(comuna);	
			_context.Comunas.Add(comuna);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetComuna), new { id = comuna.IdComuna }, comunaDTO);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutComuna(int id, ComunaDTO comunaDTO)
		{
			var comuna = await _context.Comunas
				.Include(c => c.Region)
				.ThenInclude(r => r.Comunas)
				.FirstOrDefaultAsync(c=> c.IdComuna == id);
			if (comuna == null)
				return NotFound();
			comuna.Region.Comunas.Remove(comuna);
			var region = await _context.Regiones
				.Include(r => r.Comunas)
				.FirstOrDefaultAsync(r => r.IdRegion == comunaDTO.IdRegion);
			if (region == null)
				return NotFound("La region especificada no existe");

			comuna.Nombre = comunaDTO.Nombre;
			comuna.IdRegion = comunaDTO.IdRegion;
			comuna.Region = region;
			region.Comunas.Add(comuna);

			_context.Entry(comuna).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ComunaExist(id))
					return NotFound();
				else
					throw;
			}

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteComuna(int id)
		{
			var comuna = await _context.Comunas
				.Include(c => c.Region)
				.ThenInclude(r => r.Comunas)
				.FirstOrDefaultAsync(c => c.IdComuna == id);
			if (comuna == null)
				return NotFound();
			comuna.Region.Comunas.Remove(comuna);
			_context.Comunas.Remove(comuna);
			await _context.SaveChangesAsync();

			return NoContent();
		}
		private bool ComunaExist(int id)
		{
			return _context.Comunas.Any(e => e.IdComuna == id);
		}
	}
	public class ComunaDTO
	{
		public int IdRegion { get; set; }
		public string Nombre { get; set; }
	}
	public class ComunaDTOGet
	{
		public int IdComuna { get; set; }
		public int IdRegion { get; set; }
		public string Nombre { get; set; }
		public string NombreRegion { get; set; }
		public List<string> NombreServicios { get; set; }
	}
}