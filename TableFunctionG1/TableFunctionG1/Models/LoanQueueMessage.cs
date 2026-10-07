// this class dictates what the structure of each message being added into the queue is going to look like
// we map from this class (using DTO mapping), into our Loan Table Entity class
namespace TableFunctionG1.Models
{
	public class LoanQueueMessage
	{
		public string BookId { get; set; } = string.Empty;
		public string BorrowerId { get; set; } = string.Empty;
		public DateTime LoanDate { get; set; } = DateTime.Now;
	}
}
