using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PruebaTecnicaLisit.Models.Application;
using PruebaTecnicaLisit.Models.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PruebaTecnicaLisit.Controllers
{
    [ApiController]
	[Route("api/[controller]")]
	public class AccountController : ControllerBase
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _dbContext;
		private readonly LoggerService _logger;

		public AccountController(SignInManager<ApplicationUser> signInManager,
								UserManager<ApplicationUser> userManager,
								ApplicationDbContext dbContext,
								LoggerService logger
			)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_dbContext = dbContext;
			_logger = logger;
		}
		/// <summary>
		/// Obtiene los datos de todos los usuarios.
		/// </summary>
		/// <remarks>
		/// Obtiene la comuna, el e-mail y el Id de cada usuario existente. Solo es accesible por un Adminsitrador.
		/// </remarks>
		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			var users = _dbContext.Users
				.Include(u => u.Comuna)
				.Select(u => new
				{
					IdUsuario = u.Id,
					Email = u.Email,
					Comuna = u.Comuna.Nombre,
					IdComuna = u.Comuna.IdComuna
				})
				.ToList();
			_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString());

			return Ok(users);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginData loginData)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _signInManager.PasswordSignInAsync(loginData.Email, loginData.Password, true, lockoutOnFailure: false);

			if (result.Succeeded)
			{
				var user = await _userManager.FindByEmailAsync(loginData.Email);
				var tokenString = GenerateJwtToken(user);
				_logger.LogAction(user.Email, ControllerContext.RouteData.Values["action"].ToString());

				return Ok(new { token = tokenString, idUsuario = user.Id });
			}

			return BadRequest(new { message = "Email o contraseña inválidos" });
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterData registerData)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var comuna = await _dbContext.Comunas.FindAsync(registerData.IdComuna);
			if (comuna is null) 
				return NotFound("La Comuna especificada no existe");

			var user = new ApplicationUser { UserName = registerData.Email, Email = registerData.Email, IdComuna = registerData.IdComuna, Comuna = comuna };
			var result = await _userManager.CreateAsync(user, registerData.Password);
			if (result.Succeeded)
			{
				_logger.LogAction(user.Email, ControllerContext.RouteData.Values["action"].ToString());
				return Ok(new { message = "Registrado correctamente" });
			}
			return BadRequest(new { message = "Falló al registrarse" });
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return Ok("Sesión cerrada exitosamente");
		}
		private string GenerateJwtToken(ApplicationUser user)
		{
			bool isAdmin = _userManager.IsInRoleAsync(user, "Admin").Result;
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes("c3p6V3IwODVxTmRHVXhJdE82SDVwTkhZVEE1Z1loS0VZTVo2ZkRWTjZkOW1aVzZMZ1FrS3oyV25iUjZM");
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, user.Id),
					new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "Normal"),
				}),
				Expires = DateTime.UtcNow.AddDays(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var tokenString = tokenHandler.WriteToken(token);

			return tokenString;
		}
		[Authorize(Roles = "Admin")]
		[HttpPost("asignar-admin/{userId}")]
		public async Task<IActionResult> AssignAdminRole(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound(new { message = "User not found" });

			var result = await _userManager.AddToRoleAsync(user, "Admin");
			if (result.Succeeded)
			{
				_logger.LogAction(_userManager.GetUserName(User), ControllerContext.RouteData.Values["action"].ToString(), userId);
				return Ok(new { message = "Admin role assigned successfully" });
			}
			else
				return BadRequest(new { message = "Failed to assign Admin role", errors = result.Errors });
		}	

	}

	public class LoginData
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
	public class RegisterData
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public int IdComuna { get; set; }
	}
}
