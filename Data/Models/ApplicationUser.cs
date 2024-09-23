using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ExpenseApplication.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ManagerId { get; set; }
		[JsonIgnore] // Prevent serialization to avoid circular reference
		public List<ExpenseForm> ExpenseForms { get; set; }
	}
}
