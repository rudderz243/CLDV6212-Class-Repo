using System;
using System.Collections.Generic;
using System.Text;

namespace TableFunctionG2.Models
{
	public class LoanQueueMessage
	{
		public string BookId { get; set; } = string.Empty;
		public string BorrowerId { get; set; } = string.Empty;
		public DateTime LoanDate { get; set; } = DateTime.Now;
	}
}
