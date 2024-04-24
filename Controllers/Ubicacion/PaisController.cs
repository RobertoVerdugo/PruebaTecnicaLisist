using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Ubicacion;
using PruebaTecnicaLisit.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace PruebaTecnicaLisit.Controllers.Ubicacion
{
	[Route("api/[controller]")]
	//[Authorize(Roles = "Admin")]
	[ApiController]
	public class PaisController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public PaisController(ApplicationDbContext context)
		{
			_context = context;
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<PaisDTO>>> GetPaises()
		{
			var paises = await _context.Paises
				.Select(p => new PaisDTO
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
		public async Task<ActionResult<PaisDTO>> GetPais(int id)
		{
			var pais = await _context.Paises
				.Where(p => p.IdPais == id)
				.Select(p => new PaisDTO
				{
					IdPais = p.IdPais,
					Nombre = p.Nombre,
					NombreRegiones = p.Regiones.Select(r => r.Nombre).ToList()
				})
				.FirstOrDefaultAsync();

			if (pais == null)
				return NotFound();

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

			return CreatedAtAction(nameof(GetPais), new { id = pais.IdPais }, new {id = pais.IdPais, nombre = nombre});
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutPais(int id, string nombre)
		{
			var pais = await _context.Paises.FindAsync(id);
			if (pais == null)
				return NotFound();

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

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePais(int id)
		{
			var pais = await _context.Paises.FindAsync(id);
			if (pais == null)
				return NotFound();

			_context.Paises.Remove(pais);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool PaisExists(int id)
		{
			return _context.Paises.Any(e => e.IdPais == id);
		}
	}
	public class PaisDTO
	{
		public int IdPais { get; set; }
		public string Nombre { get; set; }
		public List<string> NombreRegiones { get; set; }
	}
}
