using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PruebaTecnicaLisit.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AccountController : ControllerBase
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;

		public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] UserData loginData)
		{
			if (loginData == null || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
			{
				return BadRequest(new { message = "Email and password are required" });
			}

			var result = await _signInManager.PasswordSignInAsync(loginData.Email, loginData.Password, true, lockoutOnFailure: false);
			if (result.Succeeded)
			{
				return Ok(new { message = "Login successful" });
			}
			return BadRequest(new { message = "Invalid email or password" });
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] UserData registerData)
		{
			if (registerData == null || string.IsNullOrEmpty(registerData.Email) || string.IsNullOrEmpty(registerData.Password))
			{
				return BadRequest(new { message = "Email and password are required" });
			}

			var user = new ApplicationUser { UserName = registerData.Email, Email = registerData.Email };
			var result = await _userManager.CreateAsync(user, registerData.Password);
			if (result.Succeeded)
			{
				return Ok(new { message = "Registration successful" });
			}
			return BadRequest(new { message = "Failed to register" });
		}
	}

	public class UserData
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
