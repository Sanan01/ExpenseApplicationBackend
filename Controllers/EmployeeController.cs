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
	[Authorize(Roles = UserRoles.Employee)]
	public class EmployeeController(EmployeeService employeeService) : ControllerBase
	{
		private readonly EmployeeService _employeeService = employeeService;

		[HttpPost("add-expense-form")]
		public IActionResult AddExpenseForm([FromBody] ExpenseFormCreateVM expenseForm)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 404,
					message: "User ID not found",
					error: "No Employee with the given user ID was found."
				);
				return NotFound(errorResponse);
			}

			try
			{
				expenseForm.ApplicationUserId = userId;

				var newExpenseForm = _employeeService.AddExpenseForm(expenseForm);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense Form Created successfully",
					data: newExpenseForm
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

		[HttpGet("get-expense-form")]
		public IActionResult GetExpenseForm(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				var errorResponse = new ApiResponse<object>(
					statusCode: 404,
					message: "User ID not found",
					error: "No Employee with the given user ID was found."
				);
				return NotFound(errorResponse);
			}

			try
			{
				var expenseForm = _employeeService.GetExpenseForms(userId, orderBy, searchKeyword, pageNumber, pageSize);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense Forms Retrieved successfully",
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
		public IActionResult UpdateExpenseForm([FromBody] ExpenseFormUpdateEmployeeVM expenseForm)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			try
			{
				var updatedExpenseForm = _employeeService.UpdateExpenseForm(expenseForm);

				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense Form Updated successfully",
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
