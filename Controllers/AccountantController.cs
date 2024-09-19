using System.Security.Claims;
using ExpenseApplication.CustomApiResponse;
using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Services;
using ExpenseApplication.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = UserRoles.Accountant)]
	public class AccountantController(AccountantService accountantService) : ControllerBase
	{
		private readonly AccountantService _accountantService = accountantService;

		[HttpGet("get-expense-form")]
		public IActionResult GetExpenseFormAccountant(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var expenseForm = _accountantService.GetExpenseForms(orderBy, searchKeyword, pageNumber, pageSize);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense Forms Retrieved Successfully",
					data: expenseForm
				);
				return Ok(successResponse);
			}
			catch (Exception ex)
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 500,
					message: "An error occurred",
					error: ex.Message
				);
				return StatusCode(500, errorResponse);
			}
		}

		[HttpPut("pay-expense-form")]
		public IActionResult PayExpenseForm([FromBody] ExpenseFormPayByAccountant expenseForm)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 404,
					message: "User ID not found",
					error: "No Accountant with the given user ID was found."
				);
				return NotFound(errorResponse);
			}

			try
			{
				expenseForm.PaidBy = userId;

				var updatedExpenseForm = _accountantService.PayExpenseForm(expenseForm);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense paid successfully",
					data: updatedExpenseForm
				);
				return Ok(successResponse);
			}
			catch (Exception ex)
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 500,
					message: "An error occurred",
					error: ex.Message
				);
				return StatusCode(500, errorResponse);
			}
		}
	}
}
