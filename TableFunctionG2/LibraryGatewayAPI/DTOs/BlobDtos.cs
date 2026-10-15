namespace LibraryGatewayAPI.DTOs
{
	public record BlobMetadataDto
	{
		public string FileName { get; init; } = string.Empty;
		public long SizeBytes { get; init; }
		public DateTimeOffset? DateModified { get; init; }
	}
}
