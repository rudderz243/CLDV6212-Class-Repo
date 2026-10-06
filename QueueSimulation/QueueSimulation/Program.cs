using QueueSimulation.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace QueueSimulation
{
	internal class Program
	{
		// a queue data structure to simulate an actual queue
		private static readonly ConcurrentQueue<Order> OrderQueue = new();
		private static int _nextId = 1;
		// a timer is a function that fires at a set interval
		private static Timer? _statusTimer;
	
		static void Main(string[] args)
		{
			_statusTimer = new Timer(UpdateOrderStatus, null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));

			bool running = true;

			while (running)
			{
				Console.WriteLine("1) Add item to queue");
				Console.WriteLine("2) View queue items");
				Console.WriteLine("3) Exit");

				string? choice = Console.ReadLine();

				switch (choice)
				{
					case "1":
						AddOrder();
						break;
					case "2":
						PrintOrders();
						break;
					case "3":
						running = false;
						break;
					default:
						Console.WriteLine("Please enter a number 1-3");
						break;
				}
			}
			// if the app is exiting, clean the timer up
			_statusTimer.Dispose();
		}

		private static void AddOrder() 
		{
			Console.WriteLine("Please enter the customers name");
			string name = Console.ReadLine();

			Console.WriteLine("Enter the total price of the order");
			decimal price = Decimal.Parse(Console.ReadLine());

			Order newOrder = new Order
			{
				Id = _nextId++,
				CustomerName = name,
				TotalPrice = price,
				Status = "Placed"
			};

			// add the order to the simulated queue
			OrderQueue.Enqueue(newOrder);
			Console.WriteLine("Order added to the queue");
		}

		private static void UpdateOrderStatus(object? state)
		{
			// loop through each and every order in the queue, and update it respectively
			foreach (var item in OrderQueue)
			{
				switch (item.Status)
				{
					case "Placed":
						item.Status = "In Progress";
						break;
					case "In Progress":
						item.Status = "Awaiting Collection";
						break;
					case "Awaiting Collection":
						item.Status = "Completed";
						break;
					case "Completed":
						break;
				}
			}
		}

		private static void PrintOrders()
		{
			if (OrderQueue.IsEmpty) 
			{
				Console.WriteLine("\nQueue is empty.");
				return;
			}
			// this option prints out the JSON in a prettier format (with indenting)
			var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
			string jsonOutput = JsonSerializer.Serialize(OrderQueue, jsonOptions);

			Console.WriteLine("\nALL QUEUE ITEMS");
			Console.WriteLine(jsonOutput);
		}
	}
}
