using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueG2.Models
{
	public class MessagePayload
	{
		// general unique identifier -> creates a unique string to identify each item
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Sender { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		// DateTime.Now -> gets the current date and time
		public DateTime SentAt { get; set; } = DateTime.Now;
	}
}
