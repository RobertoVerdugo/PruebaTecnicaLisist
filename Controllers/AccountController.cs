using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PruebaTecnicaLisit.Models;
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

		public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_dbContext = dbContext;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginData loginData)
		{
			if (loginData == null || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
			{
				return BadRequest(new { message = "Email and password are required" });
			}

			var result = await _signInManager.PasswordSignInAsync(loginData.Email, loginData.Password, true, lockoutOnFailure: false);

			if (result.Succeeded)
			{
				var user = await _userManager.FindByEmailAsync(loginData.Email);
				var tokenString = GenerateJwtToken(user);

				return Ok(new { token = tokenString });
			}

			return BadRequest(new { message = "Invalid email or password" });
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterData registerData)
		{
			if (registerData == null || string.IsNullOrEmpty(registerData.Email) || string.IsNullOrEmpty(registerData.Password))
			{
				return BadRequest(new { message = "Email and password are required" });
			}

			var user = new ApplicationUser { UserName = registerData.Email, Email = registerData.Email, IdComuna = registerData.IdComuna };
			var comuna = await _dbContext.Comunas.FindAsync(registerData.IdComuna);
			var result = await _userManager.CreateAsync(user, registerData.Password);
			if (result.Succeeded)
			{
				if(comuna is null) return BadRequest(new { message = "Failed to register - Comuna inválida" });
				user.Comuna = comuna;
				return Ok(new { message = "Registration successful" });
			}
			return BadRequest(new { message = "Failed to register" });
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
			var key = Encoding.ASCII.GetBytes("TuClaveSecretaAqui");
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
