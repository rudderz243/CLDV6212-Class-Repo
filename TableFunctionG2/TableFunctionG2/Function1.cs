using Azure.Data.Tables;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TableFunctionG2.Models;

namespace TableFunctionG2;

public class BookTableFunction
{
	// call in the table client - so that our app can communicate with the azure table (using our singleton)
	private readonly TableClient _tableClient;
	
	public BookTableFunction(TableServiceClient tableService)
	{
		// assign the table to the local variable, using our singleton
		_tableClient = tableService.GetTableClient("books");
		// create the table if it does not already exist
		_tableClient.CreateIfNotExists();
	}

	[Function("AddBook")]
	public async Task<HttpResponseData> AddBook([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "books")]
	HttpRequestData req) {
		// we get the request data from the body, and turn it from JSON into a new book entity
		var newBook = await JsonSerializer.DeserializeAsync<BookEntity>(req.Body);

		// check whether we were able to successfully decode the information
		if (newBook == null) {
			var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
			await badResponse.WriteStringAsync("error: invalid information passed");
			return badResponse;
		}

		// check if all required information is present
		if (string.IsNullOrWhiteSpace(newBook.Title) || string.IsNullOrWhiteSpace(newBook.PartitionKey) ||
		string.IsNullOrWhiteSpace(newBook.RowKey)) {
			var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
			await badResponse.WriteStringAsync("error: missing required information");
			return badResponse;
		}

		// if we pass the error checking phase, we add the book to the table
		await _tableClient.AddEntityAsync(newBook);

		// return a good response
		var goodResponse = req.CreateResponse(System.Net.HttpStatusCode.Created);
		await goodResponse.WriteStringAsync("message: book added succesfully");
		return goodResponse;
	}

	[Function("GetBooksByGenre")]
	public async Task<HttpResponseData> GetBooksByGenre([HttpTrigger(AuthorizationLevel.Anonymous, "get", 
	Route = "books/{genre}")] HttpRequestData req, string genre) {
		// start off with an empty list
		var genreBooks = new List<BookEntity>();

		// use LINQ to query the Azure Table, to find all books that match the specified Genre
		var tableData = _tableClient.QueryAsync<BookEntity>(b => b.PartitionKey == genre);

		// using a loop, add each of the query results to the list
		await foreach (var item in tableData) {
			genreBooks.Add(item);
		}

		// return a response to the user containing a list of all the books
		var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
		await response.WriteAsJsonAsync(genreBooks);
		return response;
	}

	[Function("GetBookById")]
	public async Task<HttpResponseData> GetBookById ([HttpTrigger(AuthorizationLevel.Anonymous, "get",
	Route = "books/{genre}/{id}")] HttpRequestData req, string genre, string id) {
	
	}




}