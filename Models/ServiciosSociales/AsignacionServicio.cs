using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaLisit.Models.ServiciosSociales
{
	public class AsignacionServicio
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int IdAsignacion { get; set; }
		[ForeignKey("ApplicationUser")]
		public string IdUsuario { get; set; }
		[ForeignKey("Servicio")]
		public int IdServicio { get; set; }
		public int Año { get; set; }

		public ApplicationUser Usuario { get; set; }
		public Servicio Servicio { get; set; }
	}
}
