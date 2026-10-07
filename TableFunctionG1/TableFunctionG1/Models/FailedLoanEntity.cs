using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

// the failed loan entity class is for a table entity that stores queue items that failed to get processed correctly
namespace TableFunctionG1.Models
{
	public class FailedLoanEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = "PoisonQueue";
		public string RowKey { get; set; } = Guid.NewGuid().ToString();

		// payload -> whatever the queue message was
		public string Payload { get; set; } = string.Empty;
		public string FailureReason { get; set; } = string.Empty;
		// how many times did the Consumer function _try_ to process the queue item
		public long DequeueCount { get; set; }
		public DateTimeOffset? EnqueuedTime { get; set; }

		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
	}
}
