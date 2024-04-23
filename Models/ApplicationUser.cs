using Microsoft.AspNetCore.Identity;
using PruebaTecnicaLisit.Models.ServiciosSociales;
using PruebaTecnicaLisit.Models.Ubicacion;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaLisit.Models
{
    public class ApplicationUser : IdentityUser
    {
        [ForeignKey("Comuna")]
        public int IdComuna { get; set; }
		public Comuna Comuna { get; set; }
		public ICollection<AsignacionServicio> ServiciosUsuario { get; set; }
	}
}
