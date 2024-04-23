using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaLisit.Models.Ubicacion
{
	public class Pais
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int IdPais { get; set; }
		public string Nombre { get; set; }

		public ICollection<Region> Regiones { get; set; }
	}
}
