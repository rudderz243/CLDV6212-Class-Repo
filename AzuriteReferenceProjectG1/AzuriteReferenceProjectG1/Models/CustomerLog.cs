using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzuriteReferenceProjectG1.Models
{
	public class CustomerLog : ITableEntity
	{
		// REQUIRED ATTRIBUTES FOR A TABLE ENTITY
		// PartitionKey -> general category
		public string PartitionKey { get; set; } = "Customers";
		// RowKey -> similar to the primary key in a SQL database
		public string RowKey { get; set; } = Guid.NewGuid().ToString();
		public DateTimeOffset? Timestamp { get; set; }
		public ETag ETag { get; set; }

		// OUR INFORMATION
		public string CustomerName { get; set; } = string.Empty;
		public string Action { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.Now;


		public override string ToString()
		{
			return $"[{PartitionKey} | {RowKey}] : {CustomerName} performed {Action} at {CreatedAt}";
		}
	}
}
