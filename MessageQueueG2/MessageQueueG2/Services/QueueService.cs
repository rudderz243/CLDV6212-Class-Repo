using Azure.Storage.Queues;
using MessageQueueG2.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MessageQueueG2.Services
{
	public class QueueService
	{
		// QueueClient -> how our service class will interact with the Q
		private readonly QueueClient _queueClient;

		// Constructor -> use to setup/initialize our connection to the queue
		public QueueService(string connectionString, string queueName)
		{
			_queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
			{
				// MessageEncoding -> this is the format in which we will communicate with the queue
				MessageEncoding = QueueMessageEncoding.Base64
			});
			// if the queue already exists: connect; else -> create the queue THEN connect
			_queueClient.CreateIfNotExists(); 
		}

		// SendMessageAsync -> pass through the information related to our message, into the queue
		public async Task SendMessageAsync(MessagePayload messageToSend) 
		{
			// 1: converted the object into JSON (machine readable)
			string convertedMessage = JsonSerializer.Serialize(messageToSend);
			// 2: send it off into the queue!
			await _queueClient.SendMessageAsync(convertedMessage);
		}
	}
}
