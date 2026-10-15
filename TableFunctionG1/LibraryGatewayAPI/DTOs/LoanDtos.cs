using System.ComponentModel.DataAnnotations;

namespace LibraryGatewayAPI.DTOs
{
	public record LoanSubmissionRequestDto // what we pass to the queue producer
	{
		[Required]
		public string BookId { get; init; } = string.Empty;
		[Required]
		public string BorrowerId { get; init; } = string.Empty;
	}
	public record LoanSubmissionResponseDto // what the queue producer gives back to us
	{
		public string Message { get; init; } = string.Empty;
		public DateTime QueuedAtUtc { get; init; }
	}
}
