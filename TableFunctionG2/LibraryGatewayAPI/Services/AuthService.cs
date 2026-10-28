using Azure.Data.Tables;
using LibraryGatewayAPI.DTOs;
using LibraryGatewayAPI.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryGatewayAPI.Services
{
	public class AuthService
	{
		private readonly TableClient _table; // users are stored in the table service
		private readonly IConfiguration _config; // this lets us read from appsettings.json

		public AuthService(TableServiceClient _t, IConfiguration _c)
		{
			// prep the table
			_table = _t.GetTableClient("users");
			_table.CreateIfNotExists();

			// load appsettings.json
			_config = _c;
		}

		public async Task<bool> RegisterAsync(RegistrationDto dto)
		{
			// check if the email attempting to register is already in the table
			var existing = _table.QueryAsync<UserEntity>(u => u.RowKey == dto.Email);
			// since query returns a list, if the list has items (so a match is found), we cancel the register
			await foreach (var u in existing)
			{
				return false;
			}

			// hash the users password so we can store it
			string hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);
			// create and store the new entity in the table service
			var newUser = new UserEntity
			{
				PartitionKey = dto.Role,
				RowKey = dto.Email.ToLowerInvariant(),
				FullName = dto.FullName,
				PasswordHash = hashed,
				CreatedAt = DateTime.UtcNow
			};
			// add
			await _table.AddEntityAsync(newUser);
			// return success!
			return true;
		}

		// helper method to generate our tokens
		private AuthResponseDto GenerateJwtToken(UserEntity user)
		{
			// get the settings for our JWT
			var settings = _config.GetSection("JwtSettings");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings["SecretKey"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var expires = DateTime.UtcNow.AddMinutes(double.Parse(settings["ExpirationMinutes"]));

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.RowKey),
				new Claim(JwtRegisteredClaimNames.Email, user.RowKey),
				new Claim(ClaimTypes.Name, user.FullName),
				new Claim(ClaimTypes.Role, user.PartitionKey)
			};

			// generating the actual token we will return to the user
			var token = new JwtSecurityToken(
				issuer: settings["Issuer"],
				audience: settings["Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);
			// once generated, return
			return new AuthResponseDto
			{
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Email = user.RowKey,
				FullName = user.FullName,
				Role = user.PartitionKey,
				ExpiresAtUtc = expires
			};
		}

		private async Task<AuthResponseDto?> AuthenticateUser(LoginDto dto)
		{
			// get the email of the user logging in
			string loginEmail = dto.Email.ToLowerInvariant();
			// check if the user exists
			var queryResults = _table.QueryAsync<UserEntity>(u => u.RowKey == loginEmail);
			// set to blank until we find it in the loop through the query results
			UserEntity loginUser = null;
			await foreach (var user in queryResults)
			{
				// if we find a match for the email, we grab it so we can progress further
				loginUser = user;
				break;
			}
			// if no user, return nothing
			if (loginUser == null)
			{
				return null;
			}
			// if password incorrect, return nothing
			if (!BCrypt.Net.BCrypt.Verify(dto.Password, loginUser.PasswordHash)) 
			{
				return null;
			}
			// if the user exists, AND they have provided the correct password, generate a token and login
			return GenerateJwtToken(loginUser);





			asdasdasdasd
		}
	}
}
