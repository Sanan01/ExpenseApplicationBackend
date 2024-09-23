using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApplication.Data.Services
{
	public class ManagerService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext context = context;

		public PaginatedResponse<ExpenseForm> GetExpenseForms(string managerId, string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = context.ExpenseForms
			.Include(e => e.Expenses)
			.Include(e => e.ApplicationUser)
			.AsNoTracking()
			.Join(
					context.ApplicationUsers.Where(u => u.ManagerId == managerId).Select(u => u.Id),
					e => e.ApplicationUserId,
					u => u,
					(e, u) => new { ExpenseForm = e, UserId = u }
				)
				.Where(eu => eu.ExpenseForm.Status == Status.Pending)
				.Select(eu => eu.ExpenseForm)
			.AsQueryable();

			if (!string.IsNullOrEmpty(searchKeyword))
			{
				query = query.Where(p => p.ApplicationUserId.Contains(searchKeyword));
			}

			query = (orderBy?.ToLower()) switch
			{
				"asc" => query.OrderBy(p => p.DateUpdated),
				"desc" => query.OrderByDescending(p => p.DateUpdated),
				_ => query.OrderBy(p => p.DateUpdated),
			};

			int currentPageNumber = pageNumber ?? 1;
			int currentPageSize = pageSize ?? 10;
			var paginatedList = PaginatedList<ExpenseForm>.Create(query, currentPageNumber, currentPageSize);
			return new PaginatedResponse<ExpenseForm>
			{
				Items = paginatedList,
				TotalPages = paginatedList.TotalPages,
				PageIndex = paginatedList.PageIndex,
				HasNextPage = paginatedList.HasNextPage,
				HasPreviousPage = paginatedList.HasPreviousPage
			};
		}

		public ExpenseForm UpdateExpenseForm(ExpenseFormUpdateManagerVM expenseForm)
		{
			var existingExpenseForm = context.ExpenseForms
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

			context.ExpenseHistories.Add(expenseHistory);
			context.ExpenseForms.Update(existingExpenseForm);
			context.SaveChanges();

			return existingExpenseForm;

		}
	}
}
