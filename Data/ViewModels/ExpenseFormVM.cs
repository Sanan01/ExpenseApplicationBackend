using ExpenseApplication.Data.Models;

namespace ExpenseApplication.Data.ViewModels
{
	public class ExpenseFormCreateVM
	{
		public string Currency { get; set; }
		public decimal TotalAmount { get; set; }
		public string? ApplicationUserId { get; set; }
		public List<Expense> Expenses { get; set; }
	}

	public class ExpenseFormUpdateEmployeeVM
	{
		public int Id { get; set; }
		public string Currency { get; set; }
		public decimal TotalAmount { get; set; }
		public List<Expense> Expenses { get; set; }
	}

	public class ExpenseFormUpdateManagerVM
	{
		public int Id { get; set; }
		public string Status { get; set; }
		public string? RejectionReason { get; set; }
		public string? ApprovedBy { get; set; }
		public string? RejectedBy { get; set; }
	}

	public class ExpenseFormPayByAccountant
	{
		public int Id { get; set; }
		public string PaidBy { get; set; }
	}
}
