using Azure.Data.Tables;
using AzuriteReferenceProjectG1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzuriteReferenceProjectG1.Services
{
	public class TableService
	{
		// create the TableClient that will interact with Azurite
		private readonly TableClient _tableClient;

		// Constructor -> initialize the table client object
		public TableService(string connectionString, string tableName)
		{
			// 1: create the table client object with the required information
			_tableClient = new TableClient(connectionString, tableName);
			// 2: establish the connection (creating the table if it doesn't exist)
			_tableClient.CreateIfNotExists();
		}

		// AddEntity -> create a new item in the Table
		public async Task AddEntityAsync(CustomerLog newItem)
		{
			await _tableClient.AddEntityAsync(newItem);
		}

		// GetAllEntities -> loop through every item in the Table, and return them
		public async Task<List<CustomerLog>> GetAllEntitiesAsync()
		{
			// 1: declare a blank list to hold the items
			var allRecords = new List<CustomerLog>();

			// 2: loop through each item in the table, and add them to the list
			await foreach (CustomerLog item in _tableClient.QueryAsync<CustomerLog>())
			{
				allRecords.Add(item);
			}

			// 3: return the results
			return allRecords;
		}
	}
}
