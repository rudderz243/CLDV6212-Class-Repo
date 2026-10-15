namespace LibraryGatewayAPI.DTOs
{
	public record BlobMetadataDto // this DTO will describe what each file/item in the blob container will look like
	{
		public string FileName { get; init; } = string.Empty;
		public long SizeBytes { get; init; }
		public DateTimeOffset? DateModified { get; init; }
	}
}
