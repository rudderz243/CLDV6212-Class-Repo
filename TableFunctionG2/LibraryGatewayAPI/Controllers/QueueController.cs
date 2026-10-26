using LibraryGatewayAPI.Clients;
using LibraryGatewayAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryGatewayAPI.Controllers
{
	[Route("api/v1/queue")]
	[ApiController]
	[Produces("application/json")]
	public class QueueController : ControllerBase
	{
		private readonly LoanQueueClient _q;

		public QueueController(LoanQueueClient q)
		{
			_q = q;
		}

		[HttpGet("submit")]
		[ProducesResponseType(typeof(LoanSubmissionRequestDto), StatusCodes.Status202Accepted)] // accepted -> in progress but OK
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)] // queue info bad
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // api kaboom
		public async Task<IActionResult> Enqueue ([FromBody] LoanSubmissionRequestDto dto)
		{
			var result = await _q.RequestLoanAsync(dto);

			return Accepted(new LoanSubmissionResponseDto
			{
				Message = result,
				QueuedAtUtc = DateTime.UtcNow
			});
		}
	}
}
