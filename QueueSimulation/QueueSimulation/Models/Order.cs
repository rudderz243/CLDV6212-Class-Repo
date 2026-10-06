using System;
using System.Collections.Generic;
using System.Text;

namespace QueueSimulation.Models
{
	public class Order
	{
		public int Id { get; set; }
		public string CustomerName { get; set; } = string.Empty;
		public decimal TotalPrice { get; set; }
		public string Status { get; set; } = "Placed";
	}
}
