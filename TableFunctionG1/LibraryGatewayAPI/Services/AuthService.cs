using Azure.Data.Tables;
using LibraryGatewayAPI.DTOs;
using LibraryGatewayAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryGatewayAPI.Services
{
	public class AuthService
	{
		// table service -> users are stored in the table so we need a table client
		private readonly TableClient _table;
		// IConfiguration -> this lets us read our jwtsettings from appsettings.json
		private readonly IConfiguration _config;

		public AuthService (TableServiceClient tableClient, IConfiguration config)
		{
			// link us to the users table so we can read/write users
			_table = tableClient.GetTableClient("users");
			_table.CreateIfNotExists();
			// configuration loads appsettings.json
			_config = config;
		}

		public async Task<bool> RegisterAsync(RegistrationDto dto)
		{
			// check if the email provided in the DTO already exists in the table
			var existing = _table.QueryAsync<UserEntity>(u => u.RowKey == dto.Email);
			// because query always returns an enumerable (list), we need to loop through the response
			await foreach (var u in existing)
			{
				// if there IS an existing user with a matching email, we fail the registration
				return false;
			}
			// now that we know the user doesn't already exist, we hash their password for safety
			string hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);
			// once we've hashed the password, we can store the user safely in the table
			var newUser = new UserEntity
			{
				PartitionKey = dto.Role,
				RowKey = dto.Email.ToLowerInvariant(),
				FullName = dto.FullName,
				PasswordHash = hashed,
				CreatedAt = DateTime.UtcNow
			};
			await _table.AddEntityAsync(newUser);
			// now that the user is added, we can return a success!
			return true;
		}

		public AuthResponseDto GenerateToken(UserEntity user)
		{
			var jwtSettings = _config.GetSection("JwtSettings");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expiration = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationMinutes"]));

			var claims = new []
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.RowKey),
				new Claim(JwtRegisteredClaimNames.Email, user.RowKey),
				new Claim(ClaimTypes.Name, user.FullName),
				new Claim(ClaimTypes.Role, user.PartitionKey),
			};

		}
	}
}
