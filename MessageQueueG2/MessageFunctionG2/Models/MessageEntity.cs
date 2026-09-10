using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageFunctionG2.Models
{
	public class MessageEntity : ITableEntity
	{
		// Required Attributes
		public string PartitionKey { get; set; } = string.Empty;
		public string RowKey { get; set; } = string.Empty;
		public DateTimeOffset? Timestamp { get; set; }
		public ETag ETag { get; set; }

		public string Sender { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTime SentAt { get; set; }
	}
}
