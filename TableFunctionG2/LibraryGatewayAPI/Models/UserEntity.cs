using Azure;
using Azure.Data.Tables;

namespace LibraryGatewayAPI.Models
{
	public class UserEntity : ITableEntity
	{
		// partition key -> role of the user
		public string PartitionKey { get; set; } = "Customer";
		// row key -> users email (or username)
		public string RowKey { get; set; } = string.Empty;
		// hashed password -> cannot read it if the table gets leaked
		public string PasswordHash { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
	}
}
