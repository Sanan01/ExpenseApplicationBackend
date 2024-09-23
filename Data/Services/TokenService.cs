using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.ViewModels;
using ExpenseApplication.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseApplication.Data.Services
{
	public interface ITokenService
	{
		Task<AuthResultVM> GenerateJwtToken(ApplicationUser user, string existingRefreshToken);
		JwtSecurityToken CreateJwtSecurityToken(IEnumerable<Claim> authClaims);
		Task<RefreshToken> CreateAndStoreRefreshToken(ApplicationUser user, JwtSecurityToken token);
		Task<AuthResultVM?> VerifyAndGenerateTokenAsync(RefreshTokenVM payload);
	}
	public class TokenService(IConfiguration configuration, ApplicationDbContext context, TokenValidationParameters tokenValidationParameters, UserManager<ApplicationUser> userManager) : ITokenService
	{
		private readonly IConfiguration configuration = configuration;
		private readonly ApplicationDbContext context = context;
		private readonly TokenValidationParameters tokenValidationParameters = tokenValidationParameters;
		private readonly UserManager<ApplicationUser> userManager = userManager;
		public async Task<AuthResultVM> GenerateJwtToken(ApplicationUser user, string existingRefreshToken)
		{
			var authClaims = new List<Claim>
			{
				new(ClaimTypes.Name, user.UserName),
				new(ClaimTypes.NameIdentifier, user.Id),
				new(JwtRegisteredClaimNames.UniqueName, user.UserName),
				new(JwtRegisteredClaimNames.Sub, user.UserName),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var userRoles = await userManager.GetRolesAsync(user);
			foreach (var userRole in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, userRole));
			}

			var token = CreateJwtSecurityToken(authClaims);
			var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

			var refreshToken = new RefreshToken();
			if (string.IsNullOrEmpty(existingRefreshToken))
			{
				refreshToken = await CreateAndStoreRefreshToken(user, token);
			}

			return new AuthResultVM
			{
				Token = jwtToken,
				RefreshToken = string.IsNullOrEmpty(existingRefreshToken) ? refreshToken.Token : existingRefreshToken,
				ExpiresAt = token.ValidTo
			};
		}

		public JwtSecurityToken CreateJwtSecurityToken(IEnumerable<Claim> authClaims)
		{
			var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));

			return new JwtSecurityToken(
				issuer: configuration["JwtSettings:Issuer"],
				audience: configuration["JwtSettings:Audience"],
				expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(configuration["JwtSettings:ExpiryMinutes"])),
				claims: authClaims,
				signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
			);
		}

		public async Task<RefreshToken> CreateAndStoreRefreshToken(ApplicationUser user, JwtSecurityToken token)
		{
			var refreshToken = new RefreshToken
			{
				JwtId = token.Id,
				IsRevoked = false,
				UserId = user.Id,
				CreationDate = DateTime.UtcNow,
				ExpiryDate = DateTime.UtcNow.AddMonths(6),
				Token = Guid.NewGuid().ToString()
			};

			await context.RefreshTokens.AddAsync(refreshToken);
			await context.SaveChangesAsync();

			return refreshToken;
		}

		public async Task<AuthResultVM?> VerifyAndGenerateTokenAsync(RefreshTokenVM payload)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var tokenVerification = jwtTokenHandler.ValidateToken(payload.Token, tokenValidationParameters, out var validatedToken);

				if (validatedToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				{
					throw new TokenServiceException("Invalid token algorithm.");
				}

				var utcExpiryDate = long.Parse(tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value ?? "0");
				var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

				if (expiryDate > DateTime.UtcNow)
				{
					throw new TokenServiceException("Token has not expired yet.");
				}

				var storedRefreshToken = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == payload.RefreshToken) ?? throw new Exception("Refresh token does not exist.");
				var jti = tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

				if (storedRefreshToken.JwtId != jti)
				{
					throw new TokenServiceException("Refresh token does not match JWT token.");
				}

				if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
				{
					throw new TokenServiceException("Refresh token has expired.");
				}

				if (storedRefreshToken.IsRevoked)
				{
					throw new TokenServiceException("Refresh token has been revoked.");
				}

				var user = await userManager.FindByIdAsync(storedRefreshToken.UserId);
				return user == null ? throw new TokenServiceException("User does not exist.") : await GenerateJwtToken(user, payload.RefreshToken);
			}
			catch (Exception ex)
			{
				throw new TokenServiceException("An error occurred while verifying or generating the token.", ex);
			}
		}

		private static DateTime UnixTimeStampToDateTime(long utcExpiryDate)
		{
			var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddSeconds(utcExpiryDate).ToLocalTime();
			return dateTime;
		}
	}
}
