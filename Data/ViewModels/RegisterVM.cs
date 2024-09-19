using System.ComponentModel.DataAnnotations;

namespace ExpenseApplication.Data.ViewModels
{
	public class RegisterVM
	{
		[Required(ErrorMessage = "Username is required")]
		public required string Username { get; set; }
		[Required(ErrorMessage = "Password is required")]
		public required string Password { get; set; }
		[Required(ErrorMessage = "Role is required")]
		public required string Role { get; set; }

		public string? ManagerId { get; set; }
	}
}
