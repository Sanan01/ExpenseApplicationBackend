using Azure;
using ExpenseApplication.CustomApiResponse;
using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Services;
using ExpenseApplication.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController(UserManager<ApplicationUser> userManager, ITokenService tokenService) : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager = userManager;
		private readonly ITokenService tokenService = tokenService;

		[HttpPost("register-user")]
		public async Task<IActionResult> Register([FromBody] RegisterVM payload)
		{
			if (!ModelState.IsValid)
				return BadRequest("Provide all the required fields.");

			var userExists = await userManager.FindByNameAsync(payload.Username);
			if (userExists != null)
				return BadRequest("User already exists.");

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

			var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "User registered successfully"
				);
			return Ok(successResponse);
		}


		[HttpPost("login-user")]
		public async Task<IActionResult> Login([FromBody] LoginVM payload)
		{
			if (!ModelState.IsValid)
				return BadRequest("Provide all the required fields.");

			var user = await userManager.FindByNameAsync(payload.Username);
			if (user == null || !await userManager.CheckPasswordAsync(user, payload.Password))
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 400,
					message: "An error occurred",
					error: "Invalid username or password."
				);
				return StatusCode(400, errorResponse);
			}

			var token = await tokenService.GenerateJwtToken(user, "");

			LoginResponseVM response = new()
			{
				Username = user.UserName,
				Token = token.Token,
				Role = (await userManager.GetRolesAsync(user)).FirstOrDefault(),
				ManagerId = user.ManagerId
			};

			var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "User logged in successfully",
					data: response
				);
			return Ok(successResponse);
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenVM payload)
		{
			try
			{
				var result = await tokenService.VerifyAndGenerateTokenAsync(payload);

				if (result == null)
					return BadRequest("Invalid Token");

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
