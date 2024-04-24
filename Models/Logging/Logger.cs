using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaLisit.Models.Logging
{
	public class Logger
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int IdLogger { get; set; }

		[Required]
		public string IdUsuario { get; set; }

		[Required]
		public string Action { get; set; }
		public string? Param { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public DateTime Timestamp { get; set; }
	}
}
