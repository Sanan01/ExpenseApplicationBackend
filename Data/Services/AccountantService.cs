using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApplication.Data.Services
{
	public class AccountantService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext _context = context;

		public List<ExpenseForm> GetExpenseForms(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = _context.ExpenseForms
				.Include(e => e.Expenses)
				.Where(e => e.Status == "Approved")
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
			return PaginatedList<ExpenseForm>.Create(query, currentPageNumber, currentPageSize);
		}

		public ExpenseForm PayExpenseForm(ExpenseFormPayByAccountant expenseForm)
		{
			var existingExpenseForm = _context.ExpenseForms
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

			_context.ExpenseHistories.Add(expenseHistory);
			_context.ExpenseForms.Update(existingExpenseForm);
			_context.SaveChanges();

			return existingExpenseForm;
		}
	}
}
