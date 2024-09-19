using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Pagination;
using ExpenseApplication.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApplication.Data.Services
{
	public class AdminService(ApplicationDbContext context)
	{
		private readonly ApplicationDbContext _context = context;

		public ManagerVM UpdateManagerId(ManagerVM manager)
		{
			var user = _context.ApplicationUsers
				.FirstOrDefault(e => e.Id == manager.UserId) ?? throw new KeyNotFoundException("User not found");

			user.ManagerId = manager.ManagerId;

			_context.ApplicationUsers.Update(user);
			_context.SaveChanges();

			return manager;
		}

		public List<ApplicationUser> GetUsers(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = _context.ApplicationUsers.AsQueryable();

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
			return PaginatedList<ApplicationUser>.Create(query, currentPageNumber, currentPageSize);
		}

		public List<ExpenseHistory> GetExpenseHistory(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = _context.ExpenseHistories.AsQueryable();

			if (!string.IsNullOrEmpty(searchKeyword))
			{
				query = query.Where(p => p.Action.Contains(searchKeyword));
			}
			query = (orderBy?.ToLower()) switch
			{
				"asc" => query.OrderBy(p => p.Date),
				"desc" => query.OrderByDescending(p => p.Date),
				_ => query.OrderBy(p => p.Date),
			};

			int currentPageNumber = pageNumber ?? 1;
			int currentPageSize = pageSize ?? 10;
			return PaginatedList<ExpenseHistory>.Create(query, currentPageNumber, currentPageSize);
		}

		public List<ExpenseForm> GetExpenseForms(string? orderBy, string? searchKeyword, int? pageNumber, int? pageSize)
		{
			var query = _context.ExpenseForms.Include(e => e.Expenses).AsQueryable();

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
	}
}
