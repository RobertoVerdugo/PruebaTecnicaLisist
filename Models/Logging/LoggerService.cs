using PruebaTecnicaLisit.Models.Application;

namespace PruebaTecnicaLisit.Models.Logging
{
	public class LoggerService
	{
		private readonly ApplicationDbContext _context;

		public LoggerService(ApplicationDbContext context)
		{
			_context = context;
		}

		public void LogAction(string userId, string action, string param = "")
		{
			userId = String.IsNullOrEmpty(userId) ? "Anónimo" : userId;
			var logEntry = new Logger
			{
				IdUsuario = userId,
				Action = GetActionDescription(action),
				Param = param,
				Timestamp = DateTime.Now
			};

			_context.Logs.Add(logEntry);
			_context.SaveChanges();
		}
		private string GetActionDescription(string methodName)
		{
			string action = methodName.Replace("System.", "");
			return string.Join(" ", System.Text.RegularExpressions.Regex.Split(action, @"(?<!^)(?=[A-Z])"));
		}
	}
}
