using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PruebaTecnicaLisit.Models.Application;
using PruebaTecnicaLisit.Models.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace PruebaTecnicaLisit.Controllers.Logging
{
	[Route("api/[controller]")]
	//[Authorize(Roles = "Admin")]
	[ApiController]
	public class LoggerController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		public LoggerController(ApplicationDbContext context)
		{
			_context = context;
		}
		[HttpGet("logs/{fecha}")]
		public async Task<ActionResult<IEnumerable<Logger>>> GetLogsByDate(DateTime fecha)
		{
			var logs = await _context.Logs
				.Where(l => l.Timestamp.Date == fecha.Date)
				.ToListAsync();

			return logs;
		}
	}
}
