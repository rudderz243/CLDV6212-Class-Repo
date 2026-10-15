using System.ComponentModel.DataAnnotations;

namespace LibraryGatewayAPI.DTOs
{
	public record BookCreateRequestDto // what WE pass the function
	{
		[Required]
		public string PartitionKey { get; init; } = "Fiction"; // init -> set ONCE
		[Required]
		public string Title { get; init; } = string.Empty;
		[Required]
		public string Author { get; init; } = string.Empty;
		[Range(1000, 2100)] // validation in the class -> sets what the year can be
		public int PublicationYear { get; init; }
	}

	public record BookResponseDto // what the function PASSES BACK
	{
		public string PartitionKey { get; init; } = string.Empty;
		public string RowKey{ get; init; } = string.Empty;
		public string Title{ get; init; } = string.Empty;
		public string Author{ get; init; } = string.Empty;
		public int PublicationYear { get; init; }
		public DateTimeOffset? Timestamp { get; init; }
		// even though our Function DOES return the ETag for each book, since we don't
		// really care about it in the response, we can leave it out
		// when we JsonSerializer.Deserialize, it will ignore any extra fields we don't have
	}
}
