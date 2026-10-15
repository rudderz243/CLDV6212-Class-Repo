namespace LibraryGatewayAPI.DTOs
{
	public record ErrorResponseDto
	{
		public int StatusCode { get; init; } // 4xx -> user error; 5xx -> server error
		public string Message { get; init; } = string.Empty;
		public string? Detailed { get; init; }
		public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
	}
}
