using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PruebaTecnicaLisit.Migrations;
using PruebaTecnicaLisit.Models.Application;
using PruebaTecnicaLisit.Models.Logging;
using PruebaTecnicaLisit.Models.ServiciosSociales;
using System.Security.Claims;

namespace PruebaTecnicaLisit.Controllers.ServiciosSociales
{
    [Route("api/[controller]")]
	//[Authorize(Roles = "Admin")]
	[ApiController]
	public class AsignacionServicioController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly LoggerService _logger;
		private readonly UserManager<ApplicationUser> _userManager;
		public AsignacionServicioController(ApplicationDbContext context, LoggerService logger, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_logger = logger;
			_userManager = userManager;
		}
		/// <summary>
		/// Obtiene todas las asignaciones
		/// </summary>
		/// <remarks>
		/// Obtiene todas las asignaciones existentes. Solo accesible por un Adminsitrador.
		/// </remarks>
		[HttpGet]
		//[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> GetAsignaciones()
		{
			var asignaciones = await _context.ServiciosUsuario
				.Include(a => a.Usuario)
					.ThenInclude(u => u.Comuna)
				.Include(a => a.Servicio)
				.ToListAsync();

			var asignacionesDTO = asignaciones.Select(a => new AsignacionServicioDTOGet
			{
				IdAsignacion = a.IdAsignacion,
				IdUsuario = a.IdUsuario,
				UsuarioEmail = a.Usuario.Email,
				NombreComuna = a.Usuario.Comuna.Nombre,
				NombreServicio = a.Servicio.Nombre,
				AnioAsignacion = a.Año
			}).ToList();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString());
			return Ok(asignacionesDTO);
		}
		/// <summary>
		/// Obtiene una ayuda por Id
		/// </summary>
		[HttpGet("{idAsignacion}")]
		//[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAsignacion(int idAsignacion)
		{
			var asignacion = await _context.ServiciosUsuario
				.Include(a => a.Usuario)
					.ThenInclude(u => u.Comuna)
				.Include(a => a.Servicio)
				.FirstOrDefaultAsync(a => a.IdAsignacion == idAsignacion);
			if (asignacion == null)
				return NotFound("La asignación especificada no existe");

			var dto = new AsignacionServicioDTOGet
			{
				IdAsignacion = asignacion.IdAsignacion,
				IdUsuario = asignacion.Usuario.Id,
				UsuarioEmail = asignacion.Usuario.Email,
				NombreComuna = asignacion.Usuario.Comuna.Nombre,
				NombreServicio = asignacion.Servicio.Nombre,
				AnioAsignacion = asignacion.Año
			};
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), idAsignacion.ToString());
			return Ok(dto);
		}
		/// <summary>
		/// Obtiene las ayudas de un usuario
		/// </summary>
		/// <remarks>
		/// Obtiene todas las ayudas asignadas a un usuario específico. 
		/// Un administrador tiene acceso a las ayudas de todos los usuarios.
		/// Un usuario autenticado tiene acceso a sus propias ayudas asignadas.
		/// </remarks>
		[HttpGet("asignaciones-usuario/{userId}")]
		[Authorize]
		public async Task<IActionResult> GetAsignacionesUsuario(string userId)
		{
			var usuario = await _userManager.Users
				.Include(u => u.Comuna)
				.Include(u => u.ServiciosUsuario)
					.ThenInclude(a => a.Servicio)
				.FirstOrDefaultAsync(u => u.Id == userId);
			if (usuario == null)
				return NotFound("El usuario especificado no existe");

			var esUsuarioSolicitado = usuario.Id == _userManager.GetUserName(User);
			var esAdmin = User.IsInRole("Admin");
			if (!esUsuarioSolicitado && !esAdmin)
				return Forbid(); 

			var asignaciones = usuario.ServiciosUsuario.Select(a => new AsignacionServicioDTOGet
			{
				IdAsignacion = a.IdAsignacion,
				IdUsuario = a.IdUsuario,
				UsuarioEmail = usuario.Email,
				NombreComuna = usuario.Comuna.Nombre,
				NombreServicio = a.Servicio.Nombre,
				AnioAsignacion = a.Año
			}).ToList();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), userId);
			return Ok(asignaciones);
		}

		[HttpPost]
		//[Authorize(Roles = "Admin")]
		public async Task<IActionResult> PostAsignacionServicio(AsignacionServicioDTOPost asignacionDTO)
		{
			var servicio = await _context.Servicios
				.Include(s => s.Comunas)
				.Include(s => s.ServiciosUsuario)
				.FirstOrDefaultAsync(s => s.IdServicio == asignacionDTO.IdServicio);
			if (servicio == null)
				return NotFound("El servicio especificado no existe");

			var usuario = await _userManager.Users
				.Include(u => u.Comuna)
				.Include(u => u.ServiciosUsuario)
				.FirstOrDefaultAsync(u => u.Id == asignacionDTO.IdUsuario);
			if (usuario == null)
				return NotFound("El usuario especificado no existe");

			var comunaUsuario = usuario.Comuna;
			if (comunaUsuario == null || servicio.Comunas.All(c => c.IdComuna != comunaUsuario.IdComuna))
				return BadRequest("El usuario y el servicio deben pertenecer a la misma comuna");

			var añoActual = DateTime.Now.Year;
			var asignacionExistente = await _context.ServiciosUsuario
				.FirstOrDefaultAsync(a => a.IdUsuario == asignacionDTO.IdUsuario && a.IdServicio == asignacionDTO.IdServicio && a.Año == añoActual);
			if (asignacionExistente != null)
				return BadRequest("El mismo servicio ya fue asignado al usuario en el mismo año");


			var nuevaAsignacion = new AsignacionServicio
			{
				IdUsuario = asignacionDTO.IdUsuario,
				IdServicio = asignacionDTO.IdServicio,
				Año = añoActual,
				Usuario = usuario,
				Servicio = servicio
			};
			servicio.ServiciosUsuario.Add(nuevaAsignacion);
			usuario.ServiciosUsuario.Add(nuevaAsignacion);

			_context.ServiciosUsuario.Add(nuevaAsignacion);
			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), asignacionDTO.ToString());
			return CreatedAtAction(nameof(GetAsignacion), new { idAsignacion = nuevaAsignacion.IdAsignacion }, asignacionDTO);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutAsignacionServicio(int id, AsignacionServicioDTOPut asignacionDTO)
		{
			var asignacion = await _context.ServiciosUsuario
				.Include(a => a.Usuario)
					.ThenInclude(u => u.ServiciosUsuario)
				.Include(a => a.Servicio)
					.ThenInclude(s => s.ServiciosUsuario)
				.FirstOrDefaultAsync(a => a.IdAsignacion == id);
			if (asignacion == null)
				return NotFound("La asignación especificada no existe");

			var servicio = await _context.Servicios
				.Include(s => s.Comunas)
				.FirstOrDefaultAsync(s => s.IdServicio == asignacionDTO.IdServicio);
			if (servicio == null)
				return NotFound("El servicio especificado no existe");

			var usuario = await _userManager.Users
				.Include(u => u.Comuna)
				.FirstOrDefaultAsync(u => u.Id == asignacionDTO.IdUsuario);
			if (usuario == null)
				return NotFound("El usuario especificado no existe");

			var comunaUsuario = usuario.Comuna;
			if (comunaUsuario == null || servicio.Comunas.All(c => c.IdComuna != comunaUsuario.IdComuna))
				return BadRequest("El usuario y el servicio deben pertenecer a la misma comuna");

			var asignacionExistente = await _context.ServiciosUsuario
				.FirstOrDefaultAsync(a => a.IdUsuario == asignacionDTO.IdUsuario && a.IdServicio == asignacionDTO.IdServicio && a.Año == asignacionDTO.Año);
			if (asignacionExistente != null)
				return BadRequest("El mismo servicio ya fue asignado al usuario en el mismo año");

			asignacion.Usuario.ServiciosUsuario.Remove(asignacion);
			asignacion.Servicio.ServiciosUsuario.Remove(asignacion);
			asignacion.IdUsuario = asignacionDTO.IdUsuario;
			asignacion.IdServicio = asignacionDTO.IdServicio;
			asignacion.Año = asignacionDTO.Año;
			asignacion.Usuario = usuario;
			asignacion.Servicio = servicio;
			servicio.ServiciosUsuario.Add(asignacion);
			usuario.ServiciosUsuario.Add(asignacion);
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AsignacionServicioExists(id))
					return NotFound();
				else
					throw; 
			}
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAsignacionServicio(int id)
		{
			var asignacion = await _context.ServiciosUsuario
				.Include(a=> a.Usuario)
					.ThenInclude(u => u.ServiciosUsuario)
				.Include(a => a.Servicio)
					.ThenInclude(s => s.ServiciosUsuario)
				.FirstOrDefaultAsync(a => a.IdAsignacion == id);
			if (asignacion == null)
				return NotFound();

			asignacion.Usuario.ServiciosUsuario.Remove(asignacion);
			asignacion.Servicio.ServiciosUsuario.Remove(asignacion);
			_context.ServiciosUsuario.Remove(asignacion);
			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
			return NoContent();
		}
		private bool AsignacionServicioExists(int id)
		{
			return _context.ServiciosUsuario.Any(s => s.IdServicio == id);
		}
	}
	public class AsignacionServicioDTOGet
	{
		public int IdAsignacion { get;set; }
		public string IdUsuario { get; set; }
		public string UsuarioEmail { get; set; }
		public string NombreComuna { get; set; }
		public string NombreServicio { get; set; }
		public int AnioAsignacion { get; set; }
	}
	public class AsignacionServicioDTOPost
	{
		public string IdUsuario { get; set; }
		public int IdServicio { get; set; }
		public override string ToString() { return $"IdUsuario : {IdUsuario}, IdServicio : {IdServicio}"; }
	}
	public class AsignacionServicioDTOPut
	{
		public string IdUsuario { get; set; }
		public int IdServicio { get; set; }
		public int Año { get; set; }
	}
}
