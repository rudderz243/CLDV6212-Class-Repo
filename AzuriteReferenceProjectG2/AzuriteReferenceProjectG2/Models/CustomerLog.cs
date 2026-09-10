using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzuriteReferenceProjectG2.Models
{
	// inherit from ITableEntity -> this is what the data in the Azure Table Storage will look like
	public class CustomerLog : ITableEntity
	{
		// REQUIRED ADDTRIBUTES FOR ITABLEENTITY
		// PartitionKey -> General category
		public string PartitionKey { get; set; } = "Customers";
		// RowKey -> similar to the Primary Key in a SQL database ; It identifies each object in the Table Storage
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
