using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApplication.Data.Services
{
	public class AccountantService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext context = context;

		public PaginatedResponse<ExpenseForm> GetExpenseForms(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = context.ExpenseForms
				.Include(e => e.Expenses)
				.Include(e => e.ApplicationUser)
				.Where(e => e.Status == Status.Approved)
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchKeyword))
			{
				query = query.Where(p => p.Status.Contains(searchKeyword));
			}

			query = (orderBy?.ToLower()) switch
			{
				"asc" => query.OrderBy(p => p.DateUpdated),
				"desc" => query.OrderByDescending(p => p.DateUpdated),
				_ => query.OrderBy(p => p.DateUpdated),
			};

			int currentPageNumber = pageNumber ?? 1;
			int currentPageSize = pageSize ?? 10;
			var PaginatedList = PaginatedList<ExpenseForm>.Create(query, currentPageNumber, currentPageSize);
			return new PaginatedResponse<ExpenseForm>
			{
				Items = PaginatedList,
				TotalPages = PaginatedList.TotalPages,
				PageIndex = PaginatedList.PageIndex,
				HasNextPage = PaginatedList.HasNextPage,
				HasPreviousPage = PaginatedList.HasPreviousPage
			};
		}

		public ExpenseForm PayExpenseForm(ExpenseFormPayByAccountant expenseForm)
		{
			var existingExpenseForm = context.ExpenseForms
				.Include(e => e.Expenses)
				.FirstOrDefault(e => e.Id == expenseForm.Id) ?? throw new KeyNotFoundException("Expense form not found");

			existingExpenseForm.Status = Status.Paid;
			existingExpenseForm.DateUpdated = DateTime.Now;
			existingExpenseForm.DatePaidAt = DateTime.Now;
			existingExpenseForm.PaidBy = expenseForm.PaidBy;

			var expenseHistory = new ExpenseHistory
			{
				Action = "Paid",
				Date = DateTime.Now,
				Comment = $"Paid by {existingExpenseForm.PaidBy}",
				UserId = existingExpenseForm.ApplicationUserId,
				ManagerId = existingExpenseForm.ApprovedBy,
				ExpenseFormId = existingExpenseForm.Id
			};

			context.ExpenseHistories.Add(expenseHistory);
			context.ExpenseForms.Update(existingExpenseForm);
			context.SaveChanges();

			return existingExpenseForm;
		}
	}
}
