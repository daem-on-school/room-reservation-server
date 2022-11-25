using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoomReservation.Model;

namespace RoomReservation.Controllers
{
    [ApiController]
    [Route("[controller]")]
	public class AuthController : ControllerBase {
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IHostEnvironment _env;
		private readonly RoleManager<IdentityRole> _roleManager;

		private readonly List<string> _roles = new List<string> { "Admin", "Organiser" };

		public AuthController(
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager,
			IHostEnvironment env,
			RoleManager<IdentityRole> roleManager
		) {
			_userManager = userManager;
			_signInManager = signInManager;
			_env = env;
			_roleManager = roleManager;

			Task task = CreateRoles();
		}

		private async Task CreateRoles() {
			foreach (var role in _roles) {
				var roleExists = await _roleManager.RoleExistsAsync(role);
				if (!roleExists) {
					await _roleManager.CreateAsync(new IdentityRole(role));
				}
			}
		}

		[HttpPost("Login")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(
			StatusCodes.Status401Unauthorized,
			Type = typeof(Microsoft.AspNetCore.Identity.SignInResult)
		)]
		public async Task<ActionResult<List<string>>> Login([FromBody] LoginDTO loginDTO) {
			var result = await _signInManager.PasswordSignInAsync(loginDTO.Username, loginDTO.Password, false, false);
			if (result.Succeeded) {
				var user = await _userManager.FindByNameAsync(loginDTO.Username);
				return Ok(await _userManager.GetRolesAsync(user));
			}
			return Unauthorized(result);
		}

		[HttpPost("Logout")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Logout() {
			await _signInManager.SignOutAsync();
			return Ok();
		}

		[HttpPost("Register")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register([FromBody] RegisterDTO data) {
			if (!_env.IsDevelopment()
				|| data.AdminToken == null
				|| data.AdminToken != Environment.GetEnvironmentVariable("AdminToken")) {
				return BadRequest("Invalid AdminToken");
			}
			if (!_roles.Contains(data.Role)) {
				return BadRequest();
			}
			var user = new IdentityUser {
				UserName = data.Username,
				Email = data.Email,
				EmailConfirmed = true,
			};
			var result = await _userManager.CreateAsync(user, data.Password);
			if (result.Succeeded) {
				await _userManager.AddToRoleAsync(user, data.Role);
				return Ok();
			}
			return Conflict(result.Errors);
		}
	}
}