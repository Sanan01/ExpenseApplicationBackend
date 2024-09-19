using ExpenseApplication.Data.Models;
using Microsoft.AspNetCore.Identity;
namespace ExpenseApplication.Data
{
	public class ApplicationDbInitializer
	{
		public static async Task SeedRoles(IApplicationBuilder applicationBuilder)
		{
			using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();
			var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

			if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
			{
				await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
			}
			if (!await roleManager.RoleExistsAsync(UserRoles.Employee))
			{
				await roleManager.CreateAsync(new IdentityRole(UserRoles.Employee));
			}
			if (!await roleManager.RoleExistsAsync(UserRoles.Accountant))
			{
				await roleManager.CreateAsync(new IdentityRole(UserRoles.Accountant));
			}
			if (!await roleManager.RoleExistsAsync(UserRoles.Manager))
			{
				await roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
			}
		}
	}
}
