using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApplication.Data.Services
{
	public class EmployeeService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext context = context;
		public ExpenseForm AddExpenseForm(ExpenseFormCreateVM expenseFormVM)
		{
			ExpenseForm expenseForm = new()
			{
				Status = Status.Pending,
				Currency = expenseFormVM.Currency,
				TotalAmount = expenseFormVM.TotalAmount,
				DateCreated = DateTime.Now,
				DateUpdated = DateTime.Now,
				DateApproved = DateTime.MinValue,
				DateRejected = DateTime.MinValue,
				DatePaidAt = DateTime.MinValue,
				ApplicationUserId = expenseFormVM.ApplicationUserId,
				Expenses = expenseFormVM.Expenses.Select(expense => new Expense
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
				Comment = $"Created by {expenseForm.ApplicationUserId}",
				UserId = expenseForm.ApplicationUserId,
				ExpenseFormId = expenseForm.Id
			};

			context.ExpenseHistories.Add(expenseHistory);
			context.ExpenseForms.Add(expenseForm);
			context.SaveChanges();
			return expenseForm;
		}

		public PaginatedResponse<ExpenseForm> GetExpenseForms(string userId, string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = context.ExpenseForms
				.Include(e => e.Expenses)
				.Include(e => e.ApplicationUser)
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

		public ExpenseForm UpdateExpenseForm(ExpenseFormUpdateEmployeeVM expenseFormVM)
		{
			var existingExpenseForm = context.ExpenseForms
				.Include(e => e.Expenses)
				.FirstOrDefault(e => e.Id == expenseFormVM.Id) ?? throw new KeyNotFoundException("Expense form not found");

			existingExpenseForm.Status = Status.Pending;
			existingExpenseForm.Currency = expenseFormVM.Currency;
			existingExpenseForm.TotalAmount = expenseFormVM.TotalAmount;
			existingExpenseForm.DateUpdated = DateTime.Now;

			foreach (var updatedExpense in expenseFormVM.Expenses)
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
			var expenseIdsToKeep = expenseFormVM.Expenses.Select(e => e.Id).ToHashSet();
			var expensesToRemove = existingExpenseForm.Expenses
				.Where(e => !expenseIdsToKeep.Contains(e.Id)).ToList();

			foreach (var expenseToRemove in expensesToRemove)
			{
				context.Expense.Remove(expenseToRemove);
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

			context.ExpenseHistories.Add(expenseHistory);
			context.ExpenseForms.Update(existingExpenseForm);
			context.SaveChanges();

			return existingExpenseForm;
		}
	}
}
