using System.ComponentModel.DataAnnotations;

namespace LibraryGatewayAPI.DTOs
{
	public record LoanSubmissionRequestDto // what we send TO the function to add a loan to the queue
	{
		[Required]
		public string BookId { get; init; } = string.Empty;
		[Required]
		public string BorrowerId { get; init; } = string.Empty;
	}

	public record LoanSubmissionResponseDto // what the function SENDS BACK once we add an item to the queue
	{
		public string Message { get; init; } = string.Empty;
		public DateTime QueuedAtUtc { get; init; }
	}
}
