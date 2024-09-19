using Microsoft.AspNetCore.Identity;

namespace ExpenseApplication.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ManagerId { get; set; }
        public List<ExpenseForm> ExpenseForms { get; set; }
	}
}
