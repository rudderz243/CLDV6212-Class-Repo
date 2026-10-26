using System.ComponentModel.DataAnnotations;

namespace LibraryGatewayAPI.DTOs
{
	public record RegistrationDto
	{
		[Required, EmailAddress] // by specifying that this field is an email, we can do some automatic validation
		public string Email { get; init; } = string.Empty;
		[Required]
		public string Password { get; init; } = string.Empty;
		[Required]
		public string FullName { get; init; } = string.Empty;
		[Required]
		[RegularExpression("^(Admin|Customer|Staff)$", ErrorMessage = "Role must be Customer/Admin/Staff")]
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
		// token -> jwt. it represents who a user is (so email, password, role, etc don't have to be passed)
		public string Token { get; init; } = string.Empty;
		public string Email { get; init; } = string.Empty;
		public string FullName { get; init; } = string.Empty;
		public string Role { get; init; } = string.Empty;
		// a JWT is only valid for a small amount of time -> we store when it expires
		public DateTime ExpiresAtUtc { get; init; }
	}
}
