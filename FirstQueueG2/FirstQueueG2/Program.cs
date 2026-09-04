using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace FirstQueueG2
{
	internal class Program
	{
		// create a variable to hold the connection string
		static string connectionString = "UseDevelopmentStorage=True;";
		static string queueName = "first-queue";

		// prepare the singleton object for our queue service
		private static readonly QueueClient queue = new(
			connectionString,
			queueName,
			new QueueClientOptions
			{
				// in what FORMAT can the queue expect us to give it messages
				MessageEncoding = QueueMessageEncoding.Base64
			});

		static async Task Main(string[] args)
		{
			try {
				await queue.CreateIfNotExistsAsync();
			} catch (Exception e) {
				Console.WriteLine("Error starting or creating queue. Check Azurite and connection string");
				// exiting with a code of 1 means to exit in an error state
				// a code of 0 would mean a normal or standard exit
				Environment.Exit(1);
			}

			// loop control variable, when it changes to false we exit
			bool runLoop = true;

			while (runLoop) {
				Console.WriteLine("Example Queue App");
				Console.WriteLine("1) Add a new message to the queue");
				Console.WriteLine("2) Peek at the next message in the queue");
				Console.WriteLine("3) Process the next message in the queue");
				Console.WriteLine("4) Exit");

				Console.Write(">");
				string choice = Console.ReadLine();

				switch (choice) {
					case "1":
						Console.WriteLine("\nAdding a new queue message...");
						Console.WriteLine("Please enter the message you would like to queue:");
						string message = Console.ReadLine();

						// check if blank
						if (string.IsNullOrWhiteSpace(message))
						{
							Console.WriteLine("Message cannot be blank.");
						}
						else {
							// reciept -> confirmation of whether the message was queued or not
							SendReceipt receipt = await queue.SendMessageAsync(message);
							Console.WriteLine("Message added.\n"
							+ "Message ID: " + receipt.MessageId + "\n"
							+ "Receipt: " + receipt.PopReceipt);
						}
						break;

						// peek -> looking at a message without dealing with it
					case "2":
						// go into the stack of message, and get me the next one only
						PeekedMessage[] peeked = await queue.PeekMessagesAsync(maxMessages: 1);

						// check whether there are actually any items in the queue to look at
						if (peeked.Length == 0) {
							Console.WriteLine("No messages are currently in the queue.");
						} else {
							Console.WriteLine("Peeked ID: " + peeked[0].MessageId);
							Console.WriteLine("Peeked Message: " + peeked[0].Body);
						}
						break;
						// process -> remove the message from the queue, and deal with it
					case "3":
						QueueMessage[] messages = await queue.ReceiveMessagesAsync(
						maxMessages: 1,
						visibilityTimeout: TimeSpan.FromSeconds(30));

						if (messages.Length == 0) {
							Console.WriteLine("No messages are currently in the queue.");
						} else {
							// 1st -> get the message
							QueueMessage m = messages[0];
							// 2nd -> process or do whatever you need to do with the message
							Console.WriteLine("Processing message with ID " + m.MessageId);
							Console.WriteLine("Message Content: " + m.Body);
							// 3rd -> once done with the message, remove it from the queue entirely
							await queue.DeleteMessageAsync(m.MessageId, m.PopReceipt);
							Console.WriteLine("Message processing complete, it is now removed from the queue.");
						}
						break;
						// exit
					case "4":
						Console.WriteLine("Closing app.");
						runLoop = false;
						break;

					default:
						Console.WriteLine("You've entered an invalid option. Enter 1-4");
						break;
				}


			}
		}
	}
}
