using System.ComponentModel.DataAnnotations;

namespace ExpenseApplication.Data.ViewModels
{
	public class LoginVM
	{
		[Required(ErrorMessage = "Username is required")]
		public required string Username { get; set; }
		[Required(ErrorMessage = "Password is required")]
		public required string Password { get; set; }
	}
}
