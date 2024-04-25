using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaLisit.Models.Application;
using PruebaTecnicaLisit.Models.Logging;

namespace PruebaTecnicaLisit.Controllers.Logging
{
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin")]
	[ApiController]
	public class LoggerController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		public LoggerController(ApplicationDbContext context)
		{
			_context = context;
		}
		/// <summary>
		/// Obtiene logs por fecha.
		/// </summary>
		/// <remarks>
		/// Obtiene todos los logs para una fecha específica.
		/// </remarks>
		/// <param name="dia">Fecha en el formato 'yyyy-mm-dd'. Por ejemplo, '2024-04-24'.</param>
		/// <response code="200">Lista de logs para la fecha especificada.</response>
		[HttpGet("{dia}")]
		public ActionResult<IEnumerable<Logger>> GetLogsByDay(DateTime dia)
		{
			DateTime inicioDia = dia.Date;
			DateTime finDia = inicioDia.AddDays(1).AddTicks(-1);

			var logs = _context.Logs
				.Where(l => l.Timestamp >= inicioDia && l.Timestamp <= finDia)
				.ToList();

			return logs;
		}
		[HttpDelete("{dia}")]
		public async Task<IActionResult> DeleteLogsByDate(DateTime dia)
		{
			DateTime fechaInicio = dia.Date;
			DateTime fechaFin = fechaInicio.AddDays(1);

			var logsToDelete = await _context.Logs
				.Where(l => l.Timestamp >= fechaInicio && l.Timestamp < fechaFin)
				.ToListAsync();

			if (logsToDelete == null || !logsToDelete.Any())
			{
				return NotFound();
			}

			_context.Logs.RemoveRange(logsToDelete);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}
