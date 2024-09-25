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
	public class AdminController(AdminService adminService, ApiResponseService apiResponseService) : ControllerBase
	{
		private readonly AdminService adminService = adminService;
		private readonly ApiResponseService apiResponseService = apiResponseService;

		[HttpGet("users")]
		public IActionResult GetUsers(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var users = adminService.GetUsers(orderBy, searchKeyword, pageNumber, pageSize);
				return apiResponseService.ApiResponseSuccess("Users retrieved successfully", users);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}

		[HttpGet("expense-history")]
		public IActionResult GetExpenseHistory(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var expenseHistory = adminService.GetExpenseHistory(orderBy, searchKeyword, pageNumber, pageSize);
				return apiResponseService.ApiResponseSuccess("Expense history retrieved successfully", expenseHistory);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}

		[HttpGet("expense-forms")]
		public IActionResult GetExpenseForms(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var expenseForms = adminService.GetExpenseForms(orderBy, searchKeyword, pageNumber, pageSize);
				return apiResponseService.ApiResponseSuccess("Expense forms retrieved successfully", expenseForms);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}

		[HttpPut("update-managerid")]
		public IActionResult UpdateManagerId([FromBody] ManagerVM manager)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var updatedExpenseForm = adminService.UpdateManagerId(manager);
				return apiResponseService.ApiResponseSuccess("Manager Id Updated successfully", updatedExpenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}
	}
}
