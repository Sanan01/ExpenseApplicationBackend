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
	[Authorize(Roles = UserRoles.Manager)]
	public class ManagerController(ManagerService managerService, ApiResponseService apiResponseService) : ControllerBase
	{
		private readonly ManagerService managerService = managerService;
		private readonly ApiResponseService apiResponseService = apiResponseService;

		[HttpGet("get-expense-form")]
		public IActionResult GetExpenseForm(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return apiResponseService.ApiResponseNotFound("User ID not found", "No Manager with the given manager ID was found.");
			}

			try
			{
				var expenseForm = managerService.GetExpenseForms(userId, orderBy, searchKeyword, pageNumber, pageSize);
				return apiResponseService.ApiResponseSuccess("Expense Forms Retrieved Successfully", expenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
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
				return apiResponseService.ApiResponseNotFound("User ID not found", "No Manager with the given manager ID was found.");
			}

			try
			{
				expenseForm.ApprovedBy = userId;
				expenseForm.RejectedBy = userId;

				var updatedExpenseForm = managerService.UpdateExpenseForm(expenseForm);
				return apiResponseService.ApiResponseSuccess("Expense updated successfully", updatedExpenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}
	}
}
