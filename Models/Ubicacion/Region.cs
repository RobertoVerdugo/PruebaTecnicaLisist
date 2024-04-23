using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PruebaTecnicaLisit.Models.Ubicacion
{
	public class Region
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int IdRegion { get; set; }
		[ForeignKey("Pais")]
		public int IdPais { get; set; }
		public string Nombre { get; set; }
		public Pais Pais { get; set; }
		public ICollection<Comuna> Comunas { get; set; }
	}
}
