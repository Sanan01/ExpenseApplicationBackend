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
		private readonly UserManager<ApplicationUser> _userManager = userManager;
		private readonly ITokenService _tokenService = tokenService;

		[HttpPost("register-user")]
		public async Task<IActionResult> Register([FromBody] RegisterVM payload)
		{
			if (!ModelState.IsValid)
				return BadRequest("Provide all the required fields.");

			var userExists = await _userManager.FindByNameAsync(payload.Username);
			if (userExists != null)
				return BadRequest("User already exists.");

			var user = new ApplicationUser
			{
				SecurityStamp = Guid.NewGuid().ToString(),
				UserName = payload.Username,
				ManagerId = payload.ManagerId
			};

			var result = await _userManager.CreateAsync(user, payload.Password);

			if (!result.Succeeded)
				return BadRequest(result.Errors);

			switch (payload.Role)
			{
				case UserRoles.Admin:
					await _userManager.AddToRoleAsync(user, UserRoles.Admin);
					break;
				case UserRoles.Manager:
					await _userManager.AddToRoleAsync(user, UserRoles.Manager);
					break;
				case UserRoles.Accountant:
					await _userManager.AddToRoleAsync(user, UserRoles.Accountant);
					break;
				default:
					await _userManager.AddToRoleAsync(user, UserRoles.Employee);
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

			var user = await _userManager.FindByNameAsync(payload.Username);
			if (user == null || !await _userManager.CheckPasswordAsync(user, payload.Password))
				return Unauthorized("Invalid Authentication");

			var token = await _tokenService.GenerateJwtToken(user, "");

			LoginResponseVM response = new()
			{
				Username = user.UserName,
				Token = token.Token,
				Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
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
				var result = await _tokenService.VerifyAndGenerateTokenAsync(payload);

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
