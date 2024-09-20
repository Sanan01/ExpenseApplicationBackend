using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApplication.Data.Services
{
	public class EmployeeService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext _context = context;
		public ExpenseForm AddExpenseForm(ExpenseFormCreateVM expenseForm)
		{
			ExpenseForm _expenseForm = new()
			{
				Status = Status.Pending,
				Currency = expenseForm.Currency,
				TotalAmount = expenseForm.TotalAmount,
				DateCreated = DateTime.Now,
				DateUpdated = DateTime.Now,
				DateApproved = DateTime.MinValue,
				DateRejected = DateTime.MinValue,
				DatePaidAt = DateTime.MinValue,
				ApplicationUserId = expenseForm.ApplicationUserId,
				Expenses = expenseForm.Expenses.Select(expense => new Expense
				{
					Type = expense.Type,
					Title = expense.Title,
					Description = expense.Description,
					Amount = expense.Amount
				}).ToList()
			};

			var expenseHistory = new ExpenseHistory
			{
				Action = "Created",
				Date = DateTime.Now,
				Comment = $"Created by {_expenseForm.ApplicationUserId}",
				UserId = _expenseForm.ApplicationUserId,
				ExpenseFormId = _expenseForm.Id
			};

			_context.ExpenseHistories.Add(expenseHistory);
			_context.ExpenseForms.Add(_expenseForm);
			_context.SaveChanges();
			return _expenseForm;
		}

		public PaginatedResponse<ExpenseForm> GetExpenseForms(string userId, string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = _context.ExpenseForms
				.Include(e => e.Expenses)
				.Where(e => e.ApplicationUserId == userId)
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

		public ExpenseForm UpdateExpenseForm(ExpenseFormUpdateEmployeeVM expenseForm)
		{
			var existingExpenseForm = _context.ExpenseForms
				.Include(e => e.Expenses)
				.FirstOrDefault(e => e.Id == expenseForm.Id) ?? throw new KeyNotFoundException("Expense form not found");

			existingExpenseForm.Status = Status.Pending;
			existingExpenseForm.Currency = expenseForm.Currency;
			existingExpenseForm.TotalAmount = expenseForm.TotalAmount;
			existingExpenseForm.DateUpdated = DateTime.Now;

			foreach (var updatedExpense in expenseForm.Expenses)
			{
				var existingExpense = existingExpenseForm.Expenses
					.FirstOrDefault(e => e.Id == updatedExpense.Id);

				if (existingExpense != null)
				{
					// Update existing expense properties
					existingExpense.Type = updatedExpense.Type;
					existingExpense.Title = updatedExpense.Title;
					existingExpense.Description = updatedExpense.Description;
					existingExpense.Amount = updatedExpense.Amount;
				}
				else
				{
					// Handle the case where the expense does not exist (optional)
					existingExpenseForm.Expenses.Add(new Expense
					{
						Type = updatedExpense.Type,
						Title = updatedExpense.Title,
						Description = updatedExpense.Description,
						Amount = updatedExpense.Amount,
						ExpenseFormId = existingExpenseForm.Id
					});
				}
			}

			// Remove expenses that are no longer part of the update (optional)
			var expenseIdsToKeep = expenseForm.Expenses.Select(e => e.Id).ToHashSet();
			var expensesToRemove = existingExpenseForm.Expenses
				.Where(e => !expenseIdsToKeep.Contains(e.Id)).ToList();

			foreach (var expenseToRemove in expensesToRemove)
			{
				_context.Expense.Remove(expenseToRemove);
			}

			var expenseHistory = new ExpenseHistory
			{
				Action = "Updated",
				Date = DateTime.Now,
				Comment = $"Updated by {existingExpenseForm.ApplicationUserId}",
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
