namespace LibraryGatewayAPI.DTOs
{
	public record ErrorResponseDto
	{
		public int StatusCode { get; init; } // 400 -> user error; 500 -> server error
		public string Message { get; init; } = string.Empty;
		public string? Detailed { get; init; } // this allows us to optionally pass through additional info
		public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
	}
}
