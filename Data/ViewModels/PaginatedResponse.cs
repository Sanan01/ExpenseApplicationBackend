namespace ExpenseApplication.Data.ViewModels
{
	public class PaginatedResponse<T>
	{
		public List<T> Items { get; set; }
		public int TotalPages { get; set; }
		public int PageIndex { get; set; }
		public bool HasNextPage { get; set; }
		public bool HasPreviousPage { get; set; }
	}

}
