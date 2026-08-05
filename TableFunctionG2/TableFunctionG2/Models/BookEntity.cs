using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableFunctionG2.Models
{
	public class BookEntity : ITableEntity
	{
		// PartitionKey -> This is a category identifier that makes objects easier to find
		public string PartitionKey { get; set; } = "Fiction"; // [ Non-Fiction, Textbook ]

		// RowKey -> Is the same thing as a primary key that we are used to from a conventional DB
		public string RowKey { get; set; } = Guid.NewGuid().ToString();

		public string Title { get; set; } = string.Empty;

		public string Author { get; set; } = string.Empty;

		public int PublishedYear { get; set; } = 2000;

		public ETag ETag { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

	}
}
