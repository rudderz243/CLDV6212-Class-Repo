using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableFunctionG2.Models
{
	public class LoanEntity : ITableEntity
	{
		// partition key -> general category
		public string PartitionKey { get; set; } = DateTime.Now.ToString("yyyy-MM-d");
		// row key -> primary key
		public string RowKey { get; set; } = Guid.NewGuid().ToString();
		public string BookId { get; set; } = string.Empty;
		public string BorrowerId { get; set; } = string.Empty;
		public DateTime LoanDate { get; set; }
		public string Status { get; set; } = "Active";

		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
	}
}
