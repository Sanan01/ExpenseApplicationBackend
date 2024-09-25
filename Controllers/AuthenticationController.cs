using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Services;
using ExpenseApplication.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController(UserManager<ApplicationUser> userManager, ITokenService tokenService, ApiResponseService apiResponseService) : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager = userManager;
		private readonly ITokenService tokenService = tokenService;
		private readonly ApiResponseService apiResponseService = apiResponseService;

		[HttpPost("register-user")]
		public async Task<IActionResult> Register([FromBody] RegisterVM payload)
		{
			if (!ModelState.IsValid)
				return apiResponseService.ApiResponseBadRequest("Provide all the required fields.");

			var userExists = await userManager.FindByNameAsync(payload.Username);
			if (userExists != null)
				return apiResponseService.ApiResponseBadRequest("User already exists.");

			var user = new ApplicationUser
			{
				SecurityStamp = Guid.NewGuid().ToString(),
				UserName = payload.Username,
				ManagerId = payload.ManagerId
			};

			var result = await userManager.CreateAsync(user, payload.Password);

			if (!result.Succeeded)
				return BadRequest(result.Errors);

			switch (payload.Role)
			{
				case UserRoles.Admin:
					await userManager.AddToRoleAsync(user, UserRoles.Admin);
					break;
				case UserRoles.Manager:
					await userManager.AddToRoleAsync(user, UserRoles.Manager);
					break;
				case UserRoles.Accountant:
					await userManager.AddToRoleAsync(user, UserRoles.Accountant);
					break;
				default:
					await userManager.AddToRoleAsync(user, UserRoles.Employee);
					break;
			}
			return apiResponseService.ApiResponseSuccess("User registered successfully");
		}


		[HttpPost("login-user")]
		public async Task<IActionResult> Login([FromBody] LoginVM payload)
		{
			if (!ModelState.IsValid)
				return apiResponseService.ApiResponseBadRequest("Provide all the required fields.");

			var user = await userManager.FindByNameAsync(payload.Username);
			if (user == null || !await userManager.CheckPasswordAsync(user, payload.Password))
			{
				return apiResponseService.ApiResponseBadRequest("Invalid username or password.");
			}

			var token = await tokenService.GenerateJwtToken(user, "");

			LoginResponseVM response = new()
			{
				Username = user.UserName,
				Token = token.Token,
				Role = (await userManager.GetRolesAsync(user)).FirstOrDefault(),
				ManagerId = user.ManagerId
			};
			return apiResponseService.ApiResponseSuccess("User logged in successfully", response);
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenVM payload)
		{
			try
			{
				var result = await tokenService.VerifyAndGenerateTokenAsync(payload);

				if (result == null)
					return apiResponseService.ApiResponseBadRequest("Invalid Token");

				return Ok(result);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseBadRequest(ex.Message);
			}
		}
	}
}
