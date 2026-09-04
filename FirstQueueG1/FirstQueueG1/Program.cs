using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace FirstQueueG1
{
	internal class Program
	{
		// connectionString for the queue
		private static string connectionString = "UseDevelopmentStorage=True";
		// name for the queue
		private static string queueName = "first-queue";

		private static QueueClient queue = new QueueClient(connectionString, queueName, new QueueClientOptions
		{
			// message encoding -> tells the queue what type of "language" we will be communicating with it in
			MessageEncoding = QueueMessageEncoding.Base64
		});

		static async Task Main(string[] args)
		{
			// try connect to the queue -> if it fails, kill the app
			try {
				await queue.CreateIfNotExistsAsync();
			} catch (Exception e) {
				// if it fails -> exit app
				Environment.Exit(1);
				// exit code of 0 -> no issues :)
				// exit code of 1 -> issues :(
			}

			bool runApp = true;

			while (runApp) {
				Console.WriteLine("First Queue Example.");
				Console.WriteLine("1) Add a new message to the queue");
				Console.WriteLine("2) Peek at the next message in the queue");
				Console.WriteLine("3) Process the next message in the queue");
				Console.WriteLine("4) Exit app");

				Console.Write(">");
				string userChoice = Console.ReadLine();

				switch (userChoice) {
					// 1 -> add item
					case "1":
						Console.WriteLine("\nAdding new message to queue...");
						Console.WriteLine("Please enter what you would like to add:");
						Console.Write(">");
						string messageToAdd = Console.ReadLine();

						// check whether something was actually typed
						if (string.IsNullOrWhiteSpace(messageToAdd)) {
							Console.WriteLine("Please enter a valid message.");
						} else {
							// receipt -> holds information on whether adding the queue item was successful or not
							SendReceipt addReceipt = await queue.SendMessageAsync(messageToAdd);
							Console.WriteLine("Message added.");
							Console.WriteLine("Message ID: " + addReceipt.MessageId);
						}
						break;

					// 2 -> peek item
					case "2":
						// go into the stack of messages, and just look at what is next, without having to process it
						PeekedMessage[] peeked = await queue.PeekMessagesAsync(maxMessages: 1);

						// are there actually any messages in the queue to look at?
						if (peeked.Length == 0) {
							Console.WriteLine("There are currently no messages in the queue.");
						} else {
							Console.WriteLine("Peeked Message ID: " + peeked[0].MessageId);
							Console.WriteLine("Peeked Message Body: " + peeked[0].Body);
						}
						break;

					// 3 -> process item
					case "3":
						QueueMessage[] queueMessages = await queue.ReceiveMessagesAsync(
						maxMessages: 1,
						visibilityTimeout: TimeSpan.FromSeconds(30));

						// check if messages are actually there
						if (queueMessages.Length == 0)
						{
							Console.WriteLine("There are currently no messages in the queue.");
						} else {
							// 1 -> get the specific message we want
							QueueMessage m = queueMessages[0];

							// 2 -> perform any processing required for that item
							Console.WriteLine("Message ID: " + m.MessageId);
							Console.WriteLine("Message Body: " + m.Body);

							// 3 -> notify the queue we are done with the item, and to remove it
							await queue.DeleteMessageAsync(m.MessageId, m.PopReceipt);
							Console.WriteLine("\nItem Removed from Queue After Processing.");
						}

						break;

					// 4 -> exit app
					case "4":
						Console.WriteLine("Exiting App...");
						runApp = false;
						break;

					// default -> invalid input provided
					default:
						break;
				}
			}
		}
	}
}
