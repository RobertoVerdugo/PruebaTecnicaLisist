using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using PruebaTecnicaLisit.Models.ServiciosSociales;

namespace PruebaTecnicaLisit.Models.Ubicacion
{
	public class Comuna
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int IdComuna { get; set; }
		[ForeignKey("Region")]
		public int IdRegion { get; set; }
		public string Nombre { get; set; }
		public Region Region { get; set; }
		public ICollection<Servicio> Servicios { get; set; }
	}
}
