using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.ViewModels;
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
		private readonly IConfiguration _configuration = configuration;
		private readonly ApplicationDbContext _context = context;
		private readonly TokenValidationParameters _tokenValidationParameters = tokenValidationParameters;
		private readonly UserManager<ApplicationUser> _userManager = userManager;
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

			var userRoles = await _userManager.GetRolesAsync(user);
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
			var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

			return new JwtSecurityToken(
				issuer: _configuration["JwtSettings:Issuer"],
				audience: _configuration["JwtSettings:Audience"],
				expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:ExpiryMinutes"])),
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

			await _context.RefreshTokens.AddAsync(refreshToken);
			await _context.SaveChangesAsync();

			return refreshToken;
		}

		public async Task<AuthResultVM?> VerifyAndGenerateTokenAsync(RefreshTokenVM payload)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var tokenVerification = jwtTokenHandler.ValidateToken(payload.Token, _tokenValidationParameters, out var validatedToken);

				if (validatedToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				{
					return null;
				}

				var utcExpiryDate = long.Parse(tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value ?? "0");
				var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

				if (expiryDate > DateTime.UtcNow)
				{
					throw new Exception("Token has not expired yet.");
				}

				var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == payload.RefreshToken) ?? throw new Exception("Refresh token does not exist.");
				var jti = tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

				if (storedRefreshToken.JwtId != jti)
				{
					throw new Exception("Refresh token does not match JWT token.");
				}

				if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
				{
					throw new Exception("Refresh token has expired.");
				}

				if (storedRefreshToken.IsRevoked)
				{
					throw new Exception("Refresh token has been revoked.");
				}

				var user = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
				return user == null ? throw new Exception("User does not exist.") : await GenerateJwtToken(user, payload.RefreshToken);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
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
