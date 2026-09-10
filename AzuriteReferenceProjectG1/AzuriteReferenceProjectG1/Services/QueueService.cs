using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzuriteReferenceProjectG1.Services
{
	public class QueueService
	{
		// this queue client will communicate with the Azurite queue
		private readonly QueueClient _queueClient;

		// Constructor -> initializes and sets up the connection to the queue
		public QueueService(string connectionString, string queueName)
		{
			_queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
			{
				// MessageEncoding -> the language we use to communicate with the queue service
				MessageEncoding = QueueMessageEncoding.Base64
			});
			// once we've passed the required options, connect (and create if needed) to the queue
			_queueClient.CreateIfNotExists();
		}

		// EnqueueMessage -> add a message to the queue
		// a type of T -> the method can accept any type of object
		public async Task EnqueueMessageAsync<T>(T message)
		{
			// 1: convert the message into machine readable format (JSON)
			string convertedMessage = JsonSerializer.Serialize(message);
			// 2: add the message into the queue
			await _queueClient.SendMessageAsync(convertedMessage);
		}
		// Peeking -> looking at the next item in the queue, without dealing with that item
		public async Task<string?> PeekMessageAsync()
		{
			// 1: get the messages
			PeekedMessage[] peekedMessages = await _queueClient.PeekMessagesAsync(maxMessages: 1);

			// 2: check is there are actually any messages in the queue
			if (peekedMessages.Length == 0) 
			{
				// return nothing if there are no messages
				return null;
			}

			// 3: return the next message if there ARE any
			return peekedMessages[0].Body.ToString();
		}
	}
}
