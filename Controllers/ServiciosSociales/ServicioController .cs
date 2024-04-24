using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaLisit.Models.ServiciosSociales;
using PruebaTecnicaLisit.Models.Ubicacion;
using PruebaTecnicaLisit.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace PruebaTecnicaLisit.Controllers.ServiciosSociales
{
	[Route("api/[controller]")]
	//[Authorize(Roles = "Admin")]
	[ApiController]
	public class ServicioController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public ServicioController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ServicioDTO>>> GetServicios()
		{
			var servicios = await _context.Servicios
				.Select(s => new ServicioDTO
				{
					IdServicio = s.IdServicio,
					Nombre = s.Nombre,
					NombresComunas = s.Comunas.Select(c => c.Nombre).ToList()
				})
				.ToListAsync();

			return Ok(servicios);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<ServicioDTO>> GetServicio(int id)
		{
			var servicio = await _context.Servicios
				.Include(s => s.Comunas)
				.FirstOrDefaultAsync(s => s.IdServicio == id);

			if (servicio == null)
			{
				return NotFound();
			}

			var servicioDTO = new ServicioDTO
			{
				IdServicio = servicio.IdServicio,
				Nombre = servicio.Nombre,
				NombresComunas = servicio.Comunas.Select(c => c.Nombre).ToList()
			};

			return servicioDTO;
		}
		[HttpGet("comuna/{idComuna}")]
		public async Task<ActionResult<IEnumerable<ServicioDTO>>> GetServiciosByComuna(int idComuna)
		{
			var comuna = await _context.Comunas
				.Include(c => c.Servicios)
				.FirstOrDefaultAsync(c => c.IdComuna == idComuna);

			if (comuna == null)
				return NotFound("La comuna especificada es inválida");

			var servicios = comuna.Servicios
				.Select(s => new ServicioDTO
				{
					IdServicio = s.IdServicio,
					Nombre = s.Nombre,
					NombresComunas = s.Comunas.Select(c => c.Nombre).ToList()
				})
				.ToList();

			return Ok(servicios);
		}

		[HttpPost("region")]
		public async Task<ActionResult<Servicio>> PostServicioByRegion(ServicioConRegionDTO servicioDTO)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var region = await _context.Regiones
				.Include(r => r.Comunas)
					.ThenInclude(c => c.Servicios)
				.FirstOrDefaultAsync(r => r.IdRegion == servicioDTO.IdRegion);

			if (region == null)
				return NotFound($"La región con ID {servicioDTO.IdRegion} no fue encontrada");

			var servicio = new Servicio
			{
				Nombre = servicioDTO.Nombre,
				Comunas = new List<Comuna>()
			};

			foreach (var comuna in region.Comunas)
			{
				comuna.Servicios.Add(servicio);
				servicio.Comunas.Add(comuna); 
			}

			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetServicio), new { id = servicio.IdServicio }, servicioDTO);
		}

		[HttpPost("comuna")]
		public async Task<ActionResult<Servicio>> PostServicioByComunas(ServicioConComunasDTO servicioDTO)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var servicio = new Servicio
			{
				Nombre = servicioDTO.Nombre,
				Comunas = new List<Comuna>()
			};

			foreach (var idComuna in servicioDTO.IdComunas)
			{
				var comuna = await _context.Comunas
					.Include(c => c.Servicios)
					.FirstOrDefaultAsync(c => c.IdComuna == idComuna);
				if (comuna == null)
					return NotFound($"La comuna con ID {idComuna} no fue encontrada");
				comuna.Servicios.Add(servicio);
				servicio.Comunas.Add(comuna);
			}

			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetServicio), new { id = servicio.IdServicio }, servicioDTO);
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> PutServicio(int id, ServicioConComunasDTO servicioDTO)
		{
			var servicio = await _context.Servicios
				.Include(s => s.Comunas)
				.ThenInclude(c => c.Servicios)
				.FirstOrDefaultAsync(s => s.IdServicio == id);
			if (servicio == null)
				return NotFound();

			foreach(Comuna c in servicio.Comunas)
			{
				c.Servicios.Remove(servicio);
			}

			var comunas = await _context.Comunas.Where(c => servicioDTO.IdComunas.Contains(c.IdComuna)).Include(c => c.Servicios).ToListAsync();
			if (comunas.Count() == 0)
				return NotFound("Ids de comuna inválidos");

			servicio.Nombre = servicioDTO.Nombre;
			servicio.Comunas = comunas;
			foreach(Comuna c in comunas)
			{
				c.Servicios.Add(servicio);
			}
			_context.Entry(servicio).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ServicioExists(id))
					return NotFound();
				else
					throw;
			}

			return NoContent();
		}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteServicio(int id)
		{
			var servicio = await _context.Servicios
				.Include(s => s.Comunas)
				.ThenInclude(c => c.Servicios)
				.FirstOrDefaultAsync(s => s.IdServicio == id);
			if (servicio == null)
				return NotFound();
			foreach(var comuna in servicio.Comunas)
			{
				comuna.Servicios.Remove(servicio);
			}
			_context.Servicios.Remove(servicio);
			await _context.SaveChangesAsync();

			return NoContent();
		}
		private bool ServicioExists(int id)
		{
			return _context.Servicios.Any(s => s.IdServicio == id);
		}
	}
	public class ServicioConRegionDTO
	{
		public string Nombre { get; set; }
		public int IdRegion { get; set; }
	}

	public class ServicioConComunasDTO
	{
		public string Nombre { get; set; }
		public List<int> IdComunas { get; set; }
	}
	public class ServicioDTO
	{
		public int IdServicio { get; set; }
		public string Nombre { get; set; }
		public List<string> NombresComunas { get; set; }
	}
}
