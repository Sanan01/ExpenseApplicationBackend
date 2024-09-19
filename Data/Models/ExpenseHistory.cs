namespace ExpenseApplication.Data.Models
{
	public class ExpenseHistory
	{
		public int? Id { get; set; }
		public string Action { get; set; }
		public DateTime? Date { get; set; }
		public string? Comment { get; set; }
		public string? UserId { get; set; }
		public string? ManagerId { get; set; }
		public int? ExpenseFormId { get; set; }
	}
}
