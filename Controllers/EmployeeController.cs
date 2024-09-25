using ExpenseApplication.Data.Models;
using System.Security.Claims;
using ExpenseApplication.Data.Services;
using ExpenseApplication.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = UserRoles.Employee)]
	public class EmployeeController(EmployeeService employeeService, ApiResponseService apiResponseService) : ControllerBase
	{
		private readonly EmployeeService employeeService = employeeService;
		private readonly ApiResponseService apiResponseService = apiResponseService;

		[HttpPost("add-expense-form")]
		public IActionResult AddExpenseForm([FromBody] ExpenseFormCreateVM expenseForm)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return apiResponseService.ApiResponseNotFound("User ID not found", "No Employee with the given user ID was found.");
			}

			try
			{
				expenseForm.ApplicationUserId = userId;

				var newExpenseForm = employeeService.AddExpenseForm(expenseForm);
				return apiResponseService.ApiResponseSuccess("Expense Form Created successfully", newExpenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}

		[HttpGet("get-expense-form")]
		public IActionResult GetExpenseForm(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return apiResponseService.ApiResponseNotFound("User ID not found", "No Employee with the given user ID was found.");
			}

			try
			{
				var expenseForm = employeeService.GetExpenseForms(userId, orderBy, searchKeyword, pageNumber, pageSize);
				return apiResponseService.ApiResponseSuccess("Expense Forms Retrieved Successfully", expenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}

		[HttpPut("update-expense-form")]
		public IActionResult UpdateExpenseForm([FromBody] ExpenseFormUpdateEmployeeVM expenseForm)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			try
			{
				var updatedExpenseForm = employeeService.UpdateExpenseForm(expenseForm);
				return apiResponseService.ApiResponseSuccess("Expense Form Updated successfully", updatedExpenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}
	}
}
