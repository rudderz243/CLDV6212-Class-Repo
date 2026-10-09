using Azure;
using Azure.Data.Tables;

// the failedLoanEntity class is a table entity class that will store all of the queue items that failed to get 
// processed correctly. it keeps track of what failed, when it failed, why it failed (full exception logging)
namespace TableFunctionG2.Models
{
	public class FailedLoanEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = "PoisonQueue";
		public string RowKey { get; set; } = Guid.NewGuid().ToString();

		// payload -> whatever was inside the queue message
		public string Payload { get; set; } = string.Empty;
		public string FailureReason { get; set; } = string.Empty;
		// how many times did the consumer function TRY to process the message before giving up
		public long DequeueCount { get; set; }
		public DateTimeOffset? EnqueueTime { get; set; }

		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
	}
}
