using ExpenseApplication.Data.Models;
using System.Security.Claims;
using ExpenseApplication.Data.Services;
using ExpenseApplication.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseApplication.CustomApiResponse;

namespace ExpenseApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = UserRoles.Manager)]
	public class ManagerController(ManagerService managerService) : ControllerBase
	{
		private readonly ManagerService _managerService = managerService;

		[HttpGet("get-expense-form")]
		public IActionResult GetExpenseForm(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 404,
					message: "User ID not found",
					error: "No Manager with the given manager ID was found."
				);
				return NotFound(errorResponse);
			}

			try
			{
				var expenseForm = _managerService.GetExpenseForms(userId, orderBy, searchKeyword, pageNumber, pageSize);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense retrieved successfully",
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

		[HttpPut("update-expense-form")]
		public IActionResult UpdateExpenseForm([FromBody] ExpenseFormUpdateManagerVM expenseForm)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 404,
					message: "User ID not found",
					error: "No Manager with the given manager ID was found."
				);
				return NotFound(errorResponse);
			}

			try
			{
				expenseForm.ApprovedBy = userId;
				expenseForm.RejectedBy = userId;

				var updatedExpenseForm = _managerService.UpdateExpenseForm(expenseForm);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense retrieved successfully",
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
