namespace ExpenseApplication.Data.Models
{
	public class Expense
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
		public decimal Amount { get; set; }

		// Navigation properties
		public int ExpenseFormId { get; set; }
	}
}
