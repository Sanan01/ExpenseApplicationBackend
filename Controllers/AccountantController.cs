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
	public class AccountantController(AccountantService accountantService, ApiResponseService apiResponseService) : ControllerBase
	{
		private readonly AccountantService accountantService = accountantService;
		private readonly ApiResponseService apiResponseService = apiResponseService;

		[HttpGet("get-expense-form")]
		public IActionResult GetExpenseFormAccountant(string? orderBy = null, string? searchKeyword = null, int? pageNumber = null, int? pageSize = null)
		{
			try
			{
				var expenseForm = accountantService.GetExpenseForms(orderBy, searchKeyword, pageNumber, pageSize);
				return apiResponseService.ApiResponseSuccess("Expense Forms Retrieved Successfully", expenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
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
				return apiResponseService.ApiResponseNotFound("User ID not found", "No Accountant with the given user ID was found.");
			}

			try
			{
				expenseForm.PaidBy = userId;

				var updatedExpenseForm = accountantService.PayExpenseForm(expenseForm);
				return apiResponseService.ApiResponseSuccess("Expense paid successfully", updatedExpenseForm);
			}
			catch (Exception ex)
			{
				return apiResponseService.ApiResponseError(ex.Message);
			}
		}
	}
}
