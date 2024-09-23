namespace ExpenseApplication.Data.ViewModels
{
	public class ExpenseHistoryResponseVM
	{
		public int? Id { get; set; }
		public string Action { get; set; }
		public DateTime? Date { get; set; }
		public string? Comment { get; set; }
		public string? UserId { get; set; }
		public string? UserName { get; set; }
		public string? ManagerId { get; set; }
		public int? ExpenseFormId { get; set; }
	}
}
