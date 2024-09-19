using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ExpenseApplication.Data.Models;

namespace ExpenseApplication.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
	{
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Expense>()
			   .Property(e => e.Amount)
			   .HasPrecision(18, 2);

			modelBuilder.Entity<ExpenseForm>()
			   .Property(e => e.TotalAmount)
			   .HasPrecision(18, 2);
		}

		public DbSet<ApplicationUser> ApplicationUsers { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }
		public DbSet<ExpenseHistory> ExpenseHistories { get; set; }
		public DbSet<ExpenseForm> ExpenseForms { get; set; }
		public DbSet<Expense> Expense { get; set; }
	}
}
