using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TableFunctionG1.Models;

namespace TableFunctionG1;

public class BookTableFunctions
{
	// TableClient -> our connection to the TableService using our Singleton from earlier
	private readonly TableClient _tableClient; 

	// we modify the constructor to create the connection when the function starts up
	public BookTableFunctions(TableServiceClient tableServicClient)
	{
		// creating the table for us if it does not already exist
		_tableClient = tableServicClient.GetTableClient("books");
		_tableClient.CreateIfNotExists();
	}

	[Function("CreateNewBook")]
	public async Task<HttpResponseData> CreateNewBook([HttpTrigger(AuthorizationLevel.Anonymous, "post", 
	Route = "books")] HttpRequestData req)
	{
		// get the book data from the request
		var book = await JsonSerializer.DeserializeAsync<BookEntity>(req.Body);

		if (book == null) {
			var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest); // (code 400)
			await badResponse.WriteStringAsync("Invalid book information provided.");
			return badResponse;
		}

		// if required data is missing: return 400
		if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrEmpty(book.Author)) {
			var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest); // (code 400)
			await badResponse.WriteStringAsync("Invalid book information provided.");
			return badResponse;
		}
		// if all information is valid and provided, add to table :)
		await _tableClient.AddEntityAsync(book);
		// return 201 code -> object created in table
		var createdResponse = req.CreateResponse(System.Net.HttpStatusCode.Created);
		await createdResponse.WriteAsJsonAsync(book);
		return createdResponse;
	}

	[Function("GetBooksByGenre")]
	public async Task<HttpResponseData> GetBooksByGenre([HttpTrigger(AuthorizationLevel.Anonymous, "get",
	Route="books/{genre}")] HttpRequestData req, string genre) {
		// get books from table
		var books = new List<BookEntity>();

		var results = _tableClient.QueryAsync<BookEntity>(b => b.PartitionKey == genre);

		// add each entity from the query results into our list
		await foreach (var item in results)
		{
			books.Add(item);
		}

		// return the list to the user
		var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
		await response.WriteAsJsonAsync(books);
		return response;
	}
}