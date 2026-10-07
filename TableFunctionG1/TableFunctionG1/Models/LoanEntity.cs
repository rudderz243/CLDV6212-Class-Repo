using Azure;
using Azure.Data.Tables;

// this class represents the loan entity, which is what the loan queue messages get converted into
// they get mapped form the LoanQueueMessage, into this class
namespace TableFunctionG1.Models
{
	public class LoanEntity : ITableEntity
	{
		// partition key -> general category key
		// in this example, our general category is the date that the item is being added
		public string PartitionKey { get; set; } = DateTime.Now.ToString("yyyy-MM-d");
		// row key -> primary key that identifies each record uniquely
		public string RowKey { get; set; } = Guid.NewGuid().ToString();

		public string BookId { get; set; } = string.Empty;
		public string BorrowerId { get; set; } = string.Empty;
		public DateTime LoanDate { get; set; }
		public string Status { get; set; } = "Active";

		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
	}
}
