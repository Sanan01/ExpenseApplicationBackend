namespace ExpenseApplication.Data.ViewModels
{
	public class ExpenseVM
	{
		public string Type { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
		public decimal Amount { get; set; }
		public int ExpenseFormId { get; set; }
	}
}
