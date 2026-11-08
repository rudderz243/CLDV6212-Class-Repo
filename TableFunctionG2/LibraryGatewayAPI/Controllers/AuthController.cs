using LibraryGatewayAPI.DTOs;
using LibraryGatewayAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryGatewayAPI.Controllers
{
	[ApiController]
	[Route("api/v1/auth")]
	[Produces("application/json")]
	public class AuthController : ControllerBase
	{
		private readonly AuthService _authService;

		public AuthController(AuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register([FromBody] RegistrationDto dto)
		{
			bool success = await _authService.RegisterAsync(dto);
			if (!success)
			{
				return BadRequest(new ErrorResponseDto
				{
					StatusCode = 400,
					Message = $"User with email '{dto.Email}' already exists."
				});
			}

			return StatusCode(StatusCodes.Status201Created, new { message = "User registered successfully." });
		}

		[HttpPost("login")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			var authResult = await _authService.AuthenticateUser(dto);
			if (authResult == null)
			{
				return Unauthorized(new ErrorResponseDto
				{
					StatusCode = 401,
					Message = "Invalid credentials provided."
				});
			}

			return Ok(authResult);
		}
	}
}
