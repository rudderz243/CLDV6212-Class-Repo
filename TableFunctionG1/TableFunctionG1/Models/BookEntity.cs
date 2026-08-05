using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableFunctionG1.Models
{
	// inherit from the ITableEntity interface -> a built in model class of a TableService table
	public class BookEntity : ITableEntity
	{
		// PartitionKey -> is a general category key that we use to categorize entities in the database
		public string PartitionKey { get; set; } = "Fiction"; // [ Non-Fiction, Textbook ]

		// RowKey -> This is the TableService version of a primary key. Uniquely identifies records
		public string RowKey { get; set; } = Guid.NewGuid().ToString();

		public string Title { get; set; } = string.Empty;
		public string Author { get; set; } = string.Empty;
		public int PublishedYear { get; set; } = 2000;

		// ETag -> entity tag, it is a unique identifier applied by Azure to each entity in a TableService
		public ETag ETag { get; set; }
		// Timestamp -> keeps track of either when the entity was created, OR when it was last updated 
		// if the entity changes
		public DateTimeOffset? Timestamp { get; set; }
	}
}
