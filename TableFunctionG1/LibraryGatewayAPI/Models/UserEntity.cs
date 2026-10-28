using Azure;
using Azure.Data.Tables;

namespace LibraryGatewayAPI.Models
{
	public class UserEntity : ITableEntity
	{
		// partition key -> role of the user, i.e., customer/staff/admin
		public string PartitionKey { get; set; } = "Customer";
		// row key -> email
		public string RowKey { get; set; } = string.Empty;
		// hashed password -> we cannot read or undo the hashing of a password
		public string PasswordHash { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
	}
}
