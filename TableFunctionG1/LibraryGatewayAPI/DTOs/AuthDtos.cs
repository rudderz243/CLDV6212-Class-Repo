using System.ComponentModel.DataAnnotations;

namespace LibraryGatewayAPI.DTOs
{
	public record RegistrationDto
	{
		[Required, EmailAddress] // this will automatically validate any incoming email addresses
		public string Email { get; init; } = string.Empty;
		[Required]
		public string Password { get; init; } = string.Empty;
		[Required]
		public string FullName { get; init; } = string.Empty;
		[Required]
		[RegularExpression("^(Admin|Staff|Customer)$", ErrorMessage = "Role must be Admin/Staff/Customer")]
		public string Role { get; init; } = "Customer";
	}

	public record LoginDto
	{
		[Required, EmailAddress]
		public string Email { get; init; } = string.Empty;
		[Required]
		public string Password { get; init; } = string.Empty;
	}

	public record AuthResponseDto
	{
		// token -> will contain the jwt with authenticates and authorizes the user (like a cookie does)
		public string Token { get; init; } = string.Empty;
		public string Email { get; init; } = string.Empty;
		public string FullName { get; init; } = string.Empty;
		public string Role { get; init; } = string.Empty;
		// a JWT token is only valid for a fixed amount of time
		public DateTime ExpiresAtUtc { get; init; }
	}
}
