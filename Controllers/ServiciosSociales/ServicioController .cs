using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaLisit.Models.ServiciosSociales;
using PruebaTecnicaLisit.Models.Ubicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using PruebaTecnicaLisit.Models.Application;
using Microsoft.AspNetCore.Identity;
using PruebaTecnicaLisit.Models.Logging;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace PruebaTecnicaLisit.Controllers.ServiciosSociales
{
    [Route("api/[controller]")]
	[Authorize(Roles = "Admin")]
	[ApiController]
	public class ServicioController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly LoggerService _logger;
		private readonly UserManager<ApplicationUser> _userManager;
		public ServicioController(ApplicationDbContext context, LoggerService logger, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_logger = logger;
			_userManager = userManager;
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ServicioDTOGet>>> GetServicios()
		{
			var servicios = await _context.Servicios
				.Select(s => new ServicioDTOGet
				{
					IdServicio = s.IdServicio,
					Nombre = s.Nombre,
					NombresComunas = s.Comunas.Select(c => c.Nombre).ToList()
				})
				.ToListAsync();

			return Ok(servicios);
		}
		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<ActionResult<ServicioDTOGet>> GetServicio(int id)
		{
			var servicio = await _context.Servicios
				.Include(s => s.Comunas)
				.FirstOrDefaultAsync(s => s.IdServicio == id);
			if (servicio == null)
				return NotFound("El servicio especificado no existe");

			var servicioDTO = new ServicioDTOGet
			{
				IdServicio = servicio.IdServicio,
				Nombre = servicio.Nombre,
				NombresComunas = servicio.Comunas.Select(c => c.Nombre).ToList()
			};

			return servicioDTO;
		}
		/// <summary>
		/// Obtiene los servicios disponibles en una comuna
		/// </summary>
		/// <remarks>
		/// Obtiene todos los servicios que están disponibles para una comuna
		/// </remarks>
		[AllowAnonymous]
		[HttpGet("servicios-comuna/{idComuna}")]
		public async Task<ActionResult<IEnumerable<ServicioDTOGet>>> GetServiciosByComuna(int idComuna)
		{
			var comuna = await _context.Comunas
				.Include(c => c.Servicios)
				.FirstOrDefaultAsync(c => c.IdComuna == idComuna);
			if (comuna == null)
				return NotFound("La comuna especificada no existe");

			var servicios = comuna.Servicios
				.Select(s => new ServicioDTOGet
				{
					IdServicio = s.IdServicio,
					Nombre = s.Nombre,
					NombresComunas = s.Comunas.Select(c => c.Nombre).ToList()
				})
				.ToList();

			return Ok(servicios);
		}

		/// <summary>
		/// Crea un servicio social para una región
		/// </summary>
		/// <remarks>
		/// Crea un servicio social para cada comuna perteneciente a la región indicada
		/// </remarks>
		[HttpPost("crear-servicio-region")]
		public async Task<ActionResult<Servicio>> PostServicioByRegion(ServicioDTOPostRegion servicioDTO)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var region = await _context.Regiones
				.Include(r => r.Comunas)
					.ThenInclude(c => c.Servicios)
				.FirstOrDefaultAsync(r => r.IdRegion == servicioDTO.IdRegion);
			if (region == null)
				return NotFound("La región especificada no existe");

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
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), servicioDTO.ToString());
			return CreatedAtAction(nameof(GetServicio), new { id = servicio.IdServicio }, servicioDTO);
		}
		/// <summary>
		/// Crea un servicio social para una o más comunas
		/// </summary>
		/// <remarks>
		/// Crea un servicio social para cada comuna especificada
		/// </remarks>
		/// <param name="servicioDTO.IdComunas"> Lista de Ids de Comunas. Se creará un servicio para cada Id especificado</param>
		[HttpPost("crear-servicio-comuna")]
		public async Task<ActionResult<Servicio>> PostServicioByComunas(ServicioDTOPostComuna servicioDTO)
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
					return NotFound($"La comuna con ID {idComuna} no existe");
				comuna.Servicios.Add(servicio);
				servicio.Comunas.Add(comuna);
			}

			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), servicioDTO.ToString());
			return CreatedAtAction(nameof(GetServicio), new { id = servicio.IdServicio }, servicioDTO);
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> PutServicio(int id, ServicioDTOPostComuna servicioDTO)
		{
			var servicio = await _context.Servicios
				.Include(s => s.Comunas)
				.ThenInclude(c => c.Servicios)
				.FirstOrDefaultAsync(s => s.IdServicio == id);
			if (servicio == null)
				return NotFound("El servicio especificado no existe");

			var comunasIds = servicioDTO.IdComunas;
			var comunas = await _context.Comunas
				.Where(c => comunasIds.Contains(c.IdComuna))
				.Include(c => c.Servicios)
				.ToListAsync();
			var comunasExistentesIds = comunas.Select(c => c.IdComuna);
			var comunasInexistentesIds = comunasIds.Except(comunasExistentesIds);
			if (comunasInexistentesIds.Any())
			{
				var comunasInexistentesStr = string.Join(", ", comunasInexistentesIds);
				return NotFound($"Los siguientes IDs de comuna no existen: {comunasInexistentesStr}");
			}

			foreach (Comuna c in servicio.Comunas)
			{
				c.Servicios.Remove(servicio);
			}
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
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
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
				return NotFound("El servicio especificado no existe");

			foreach(var comuna in servicio.Comunas)
			{
				comuna.Servicios.Remove(servicio);
			}
			_context.Servicios.Remove(servicio);
			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
			return NoContent();
		}
		private bool ServicioExists(int id)
		{
			return _context.Servicios.Any(s => s.IdServicio == id);
		}
	}
	public class ServicioDTOPostRegion
	{
		public string Nombre { get; set; }
		public int IdRegion { get; set; }

		public override string ToString() { return $"Nombre : {Nombre}, IdRegion: {IdRegion} "; }
	}

	public class ServicioDTOPostComuna
	{
		public string Nombre { get; set; }
		public List<int> IdComunas { get; set; }
		public override string ToString()
		{
			string idComunasStr = string.Join(", ", IdComunas);
			return $"Nombre: {Nombre}, IdComunas: [{idComunasStr}]";
		}

	}
	public class ServicioDTOGet
	{
		public int IdServicio { get; set; }
		public string Nombre { get; set; }
		public List<string> NombresComunas { get; set; }
	}
}
