using System.ComponentModel.DataAnnotations;

namespace LibraryGatewayAPI.DTOs
{
	public record BookCreateRequestDto // information WE need to pass to the API (so it can be sent to the function)
	{
		[Required]
		public string PartitionKey { get; init; } = string.Empty; // init -> once set, the value cannot change
		[Required]
		public string Title { get; init; } = string.Empty;
		[Required]
		public string Author { get; init; } = string.Empty;
		[Range(1000, 2100)] // range provides numerical validation inside of the class (so that you don't have to)
		public int PublishedYear { get; init; }
	}
	public record BookResponseDto // information the function will send back to US through the API
	{
		public string PartitionKey { get; init; } = string.Empty;
		public string RowKey { get; init; } = string.Empty;
		public string Title { get; init; } = string.Empty;
		public string Author { get; init; } = string.Empty;
		public int PublishedYear { get; init; }
		// usually, in our Entities (in the Function), we would specify ETag as well, but since the API does not
		// directly interact with the Azure Table Service (in Azurite), the ETags are not required
		public DateTimeOffset? Timestamp { get; init; }
	}
}
