using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace PruebaTecnicaLisit.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PingController : ControllerBase
	{
		[Authorize(Roles = "Admin")]
		[HttpGet("admin")]
		public IActionResult Post()
		{
			return Ok($"Pong Admin {User.IsInRole("Admin")}");
		}

		[HttpGet]
		public IActionResult Get()
		{
			return Ok("Pong Libre");
		}
	}
}
