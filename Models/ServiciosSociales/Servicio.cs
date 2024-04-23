using PruebaTecnicaLisit.Models.Ubicacion;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PruebaTecnicaLisit.Models.ServiciosSociales
{
	public class Servicio
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int IdServicio { get; set; }
		public string Nombre { get; set; }
		public ICollection<Comuna> Comunas { get; set; }
		public ICollection<AsignacionServicio> ServiciosUsuario { get; set; }
	}
}
