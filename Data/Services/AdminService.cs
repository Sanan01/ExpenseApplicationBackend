using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApplication.Data.Services
{
	public class AdminService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext context = context;

		public ManagerVM UpdateManagerId(ManagerVM manager)
		{
			var user = context.ApplicationUsers
				.FirstOrDefault(e => e.Id == manager.UserId) ?? throw new KeyNotFoundException("User not found");

			user.ManagerId = manager.ManagerId;

			context.ApplicationUsers.Update(user);
			context.SaveChanges();

			return manager;
		}

		public PaginatedResponse<ApplicationUser> GetUsers(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = context.ApplicationUsers.AsQueryable();

			if (!string.IsNullOrEmpty(searchKeyword))
			{
				query = query.Where(p => p.UserName.Contains(searchKeyword));
			}
			query = (orderBy?.ToLower()) switch
			{
				"asc" => query.OrderBy(p => p.UserName),
				"desc" => query.OrderByDescending(p => p.UserName),
				_ => query.OrderBy(p => p.UserName),
			};

			int currentPageNumber = pageNumber ?? 1;
			int currentPageSize = pageSize ?? 10;
			var paginatedList = PaginatedList<ApplicationUser>.Create(query, currentPageNumber, currentPageSize);
			return new PaginatedResponse<ApplicationUser>
			{
				Items = paginatedList,
				TotalPages = paginatedList.TotalPages,
				PageIndex = paginatedList.PageIndex,
				HasNextPage = paginatedList.HasNextPage,
				HasPreviousPage = paginatedList.HasPreviousPage
			};
		}

		public PaginatedResponse<ExpenseHistoryResponseVM> GetExpenseHistory(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			// Query ExpenseHistories and include the User data
			var query = context.ExpenseHistories
				.Join(
					context.ApplicationUsers,
					history => history.UserId,  // Foreign key in ExpenseHistory
					user => user.Id,            // Primary key in ApplicationUser
					(history, user) => new ExpenseHistoryResponseVM
					{
						Id = history.Id,
						Action = history.Action,
						Date = history.Date,
						Comment = history.Comment,
						UserId = history.UserId,
						UserName = user.UserName, // Get the UserName from the joined ApplicationUser
						ManagerId = history.ManagerId,
						ExpenseFormId = history.ExpenseFormId
					}
				).AsQueryable();

			// Apply search filter if a keyword is provided
			if (!string.IsNullOrEmpty(searchKeyword))
			{
				query = query.Where(p => p.Action.Contains(searchKeyword));
			}

			// Apply ordering based on the 'orderBy' parameter
			query = (orderBy?.ToLower()) switch
			{
				"asc" => query.OrderBy(p => p.Date),
				"desc" => query.OrderByDescending(p => p.Date),
				_ => query.OrderBy(p => p.Date),
			};

			// Pagination setup
			int currentPageNumber = pageNumber ?? 1;
			int currentPageSize = pageSize ?? 10;
			var paginatedList = PaginatedList<ExpenseHistoryResponseVM>.Create(query, currentPageNumber, currentPageSize);

			// Return the paginated response
			return new PaginatedResponse<ExpenseHistoryResponseVM>
			{
				Items = paginatedList,
				TotalPages = paginatedList.TotalPages,
				PageIndex = paginatedList.PageIndex,
				HasNextPage = paginatedList.HasNextPage,
				HasPreviousPage = paginatedList.HasPreviousPage
			};
		}


		public PaginatedResponse<ExpenseForm> GetExpenseForms(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = context.ExpenseForms
				.Include(e => e.Expenses)
				.Include(e => e.ApplicationUser)
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
	}
}
