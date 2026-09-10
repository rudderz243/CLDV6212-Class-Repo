using MessageQueueG2.Models;
using MessageQueueG2.Services;

namespace MessageQueueG2
{
	public class Program
	{
		private const string connectionString = "UseDevelopmentStorage=True;";
		private const string queueName = "incoming-messages";
		// make a SINGLE instance of the queueService class
		private static readonly QueueService _queueService = new QueueService(connectionString, queueName);


		public static async Task Main(string[] args)
		{
			Console.WriteLine("Queue Message Sender");
			
			Console.Write("Enter the name of the person sending the message: ");
			string sender = Console.ReadLine() ?? "Anonymous";

			while (true) 
			{
				Console.Write("\nEnter the message you want to send, or exit to close");
				string? input = Console.ReadLine();

				// check whether a message was actually entered
				if (string.Equals(input, "exit")) 
				{
					// break -> leave the loop, which will close the app
					break;
				}
				if (string.IsNullOrWhiteSpace(input))
				{
					Console.WriteLine("Please enter a message to send :)");
					// continue -> skip the rest of the instructions for this loop, and loop again
					continue;
				}
				
				var payload = new MessagePayload
				{
					Sender = sender,
					Content = input
				};

				await _queueService.SendMessageAsync(payload);
				Console.WriteLine($"Message sent successfully. ID: {payload.Id}");
			}
		}
	}
}
