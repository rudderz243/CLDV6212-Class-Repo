using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzuriteReferenceProjectG2.Services
{
	public class QueueService
	{
		// this will handle communication with the queue
		private readonly QueueClient _queueClient;

		// Constructor -> initialize the connection to Azurite / Queue
		public QueueService(string connectionString, string queueName) 
		{
			_queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
			{
				// MessageEncoding -> how our app will talk to the Queue (i.e., language of communication)
				MessageEncoding = QueueMessageEncoding.Base64
			});

			// once we have set up the queue (provided the required options), we create it
			_queueClient.CreateIfNotExists();
		}

		// Queue a new message into the Queue
		// a type of T -> the method can accept ANY form of object as a parameter
		public async Task EnqueueMessageAsync<T>(T payload)
		{
			// 1: turn the message into json
			string message = JsonSerializer.Serialize(payload);

			// 2: send the message into the queue
			await _queueClient.SendMessageAsync(message);
		}

		// Peeking -> looking at the next item in the queue, without processing it
		public async Task<string?> PeekMessageAsync() 
		{
			// 1: get the current list of messages in the queue
			PeekedMessage[] messages = await _queueClient.PeekMessagesAsync(maxMessages: 1);

			// 2: check if there are actually any messages
			if (messages.Length == 0) {
				// if no messages -> return null
				return null;
			}

			// 3: if there ARE messages, return the next message in line
			return messages[0].Body.ToString();
		} 
	}
}
