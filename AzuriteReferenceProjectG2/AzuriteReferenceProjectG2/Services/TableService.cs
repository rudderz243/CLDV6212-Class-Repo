using Azure.Data.Tables;
using AzuriteReferenceProjectG2.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzuriteReferenceProjectG2.Services
{
	public class TableService
	{
		// create our TableClient to interact with Azurite
		private readonly TableClient _tableClient;

		// Constructor -> initalize the Table client
		public TableService(string connectionString, string tableName) 
		{
			// 1: create the object
			_tableClient = new TableClient(connectionString, tableName);
			// 2: initialize it
			_tableClient.CreateIfNotExists();
		}

		// AddEntity -> this will add a new object into the Table
		public async Task AddEntityAsync(CustomerLog newEntity) 
		{
			await _tableClient.AddEntityAsync(newEntity);
		}

		public async Task<List<CustomerLog>> GetAllEntitiesAsync() 
		{
			// 1: blank list to hold all items
			var allRecords = new List<CustomerLog>();

		
			// 2: loop through the Table and add each item into the list
			await foreach (CustomerLog item in _tableClient.QueryAsync<CustomerLog>()) 
			{
				allRecords.Add(item);
			}

			// 3: once looping is complete, return the list of items
			return allRecords;
		}
	}
}
