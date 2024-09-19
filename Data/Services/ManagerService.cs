using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpenseApplication.Data.Services
{
	public class ManagerService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext _context = context;

		public List<ExpenseForm> GetExpenseForms(string managerId, string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = _context.ExpenseForms
			.Include(e => e.Expenses)
			.AsNoTracking()
			.Join(
					_context.ApplicationUsers.Where(u => u.ManagerId == managerId).Select(u => u.Id),
					e => e.ApplicationUserId,
					u => u,
					(e, u) => new { ExpenseForm = e, UserId = u }
				)
				.Where(eu => eu.ExpenseForm.Status == "Pending")
				.Select(eu => eu.ExpenseForm)
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

		public ExpenseForm UpdateExpenseForm(ExpenseFormUpdateManagerVM expenseForm)
		{
			var existingExpenseForm = _context.ExpenseForms
				.Include(e => e.Expenses)
				.FirstOrDefault(e => e.Id == expenseForm.Id) ?? throw new KeyNotFoundException("Expense form not found");

			existingExpenseForm.Status = expenseForm.Status;
			existingExpenseForm.DateUpdated = DateTime.Now;
			existingExpenseForm.DateApproved = DateTime.Now;
			existingExpenseForm.DateRejected = DateTime.Now;
			existingExpenseForm.ApprovedBy = expenseForm.ApprovedBy;
			existingExpenseForm.RejectedBy = expenseForm.RejectedBy;
			existingExpenseForm.RejectionReason = expenseForm.RejectionReason;

			var expenseHistory = new ExpenseHistory
			{
				Action = "Updated",
				Date = DateTime.Now,
				Comment = $"Updated by {existingExpenseForm.ApprovedBy}",
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
