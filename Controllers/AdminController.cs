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
	[Authorize(Roles = UserRoles.Admin)]
	public class AdminController(AdminService adminService) : ControllerBase
	{
		private readonly AdminService _adminService = adminService;

		[HttpGet("users")]
		public IActionResult GetUsers(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var users = _adminService.GetUsers(orderBy, searchKeyword, pageNumber, pageSize);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Users retrieved successfully",
					data: users
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

		[HttpGet("expense-history")]
		public IActionResult GetExpenseHistory(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var expenseHistory = _adminService.GetExpenseHistory(orderBy, searchKeyword, pageNumber, pageSize);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense history retrieved successfully",
					data: expenseHistory
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

		[HttpGet("expense-forms")]
		public IActionResult GetExpenseForms(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var expenseForms = _adminService.GetExpenseForms(orderBy, searchKeyword, pageNumber, pageSize);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Expense forms retrieved successfully",
					data: expenseForms
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

		[HttpPut("update-managerid")]
		public IActionResult UpdateManagerId([FromBody] ManagerVM manager)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var updatedExpenseForm = _adminService.UpdateManagerId(manager);
				var successResponse = new ApiResponse<object>(
					statusCode: 200,
					message: "Manager Id Updated successfully",
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
