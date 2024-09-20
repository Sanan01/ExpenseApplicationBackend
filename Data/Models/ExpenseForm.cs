namespace ExpenseApplication.Data.Models
{
	public class ExpenseForm
	{
		public int Id { get; set; }
		public string Status { get; set; }
		public string Currency { get; set; }
		public decimal TotalAmount { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime? DateUpdated { get; set; }
		public DateTime? DateApproved { get; set; }
		public DateTime? DateRejected { get; set; }
		public string? RejectionReason { get; set; }
		public DateTime? DatePaidAt { get; set; }
		public string? ApprovedBy { get; set; }
		public string? RejectedBy { get; set; }
		public string? PaidBy { get; set; }

		// Navigation properties
		public string ApplicationUserId { get; set; }
		public ApplicationUser? ApplicationUser { get; set; }
		public List<Expense> Expenses { get; set; }
	}
}
