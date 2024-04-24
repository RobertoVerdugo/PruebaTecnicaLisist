using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Ubicacion;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using PruebaTecnicaLisit.Models.Application;
using Microsoft.AspNetCore.Identity;
using PruebaTecnicaLisit.Models.Logging;

namespace PruebaTecnicaLisit.Controllers.Ubicacion
{
    [Route("api/[controller]")]
	//[Authorize(Roles = "Admin")]
	[ApiController]
	public class PaisController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly LoggerService _logger;
		private readonly UserManager<ApplicationUser> _userManager;

		public PaisController(ApplicationDbContext context, LoggerService logger, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_logger = logger;
			_userManager = userManager;
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<PaisDTOGet>>> GetPaises()
		{
			var paises = await _context.Paises
				.Select(p => new PaisDTOGet
				{
					IdPais = p.IdPais,
					Nombre = p.Nombre,
					NombreRegiones = p.Regiones.Select(r => r.Nombre).ToList()
				})
				.ToListAsync();

			return Ok(paises);
		}

		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<ActionResult<PaisDTOGet>> GetPais(int id)
		{
			var pais = await _context.Paises
				.Where(p => p.IdPais == id)
				.Select(p => new PaisDTOGet
				{
					IdPais = p.IdPais,
					Nombre = p.Nombre,
					NombreRegiones = p.Regiones.Select(r => r.Nombre).ToList()
				})
				.FirstOrDefaultAsync();

			if (pais == null)
				return NotFound("El país especificado no existe");

			return Ok(pais);
		}

		[HttpPost]
		public async Task<ActionResult<Pais>> PostPais(string nombre)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var pais = new Pais
			{
				Nombre = nombre
			};

			_context.Paises.Add(pais);
			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), nombre);
			return CreatedAtAction(nameof(GetPais), new { id = pais.IdPais }, new {id = pais.IdPais, nombre = nombre});
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutPais(int id, string nombre)
		{
			var pais = await _context.Paises.FindAsync(id);
			if (pais == null)
				return NotFound("El país especificado no existe");

			pais.Nombre = nombre;

			_context.Entry(pais).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!PaisExists(id))
					return NotFound();
				else
					throw;
			}
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePais(int id)
		{
			var pais = await _context.Paises.FindAsync(id);
			if (pais == null)
				return NotFound("El país especificado no existe");

			_context.Paises.Remove(pais);
			await _context.SaveChangesAsync();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), id.ToString());
			return NoContent();
		}

		private bool PaisExists(int id)
		{
			return _context.Paises.Any(e => e.IdPais == id);
		}
	}
	public class PaisDTOGet
	{
		public int IdPais { get; set; }
		public string Nombre { get; set; }
		public List<string> NombreRegiones { get; set; }
	}
}
