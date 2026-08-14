using Azure;
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
		try {
			// to get a specific item from tableStorage, you must specify the partitionkey (genre), rowkey (primary key/id)
			var requestedEntity = await _tableClient.GetEntityAsync<BookEntity>(genre, id);
			// once we get the item, we return a response to the user
			var goodResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await goodResponse.WriteAsJsonAsync(requestedEntity);
			return goodResponse;
		} catch (RequestFailedException ex) when (ex.Status == 404) {
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("error: item not found");
			return notFoundResponse;
		} catch (Exception e) {
			var genericErrorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
			await genericErrorResponse.WriteStringAsync("error: server exploded");
			return genericErrorResponse;
		}
	}

	[Function("UpdateBook")]
	public async Task<HttpResponseData> UpdateBook([HttpTrigger(AuthorizationLevel.Anonymous, "put",
	Route ="books/{genre}/{id}")] HttpRequestData req, string genre, string id) {
		// get the updated book information from the req.body
		var updatedBook = await JsonSerializer.DeserializeAsync<BookEntity>(req.Body);

		if (updatedBook == null) {
			var blankResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
			await blankResponse.WriteStringAsync("error: please provide all book information");
			return blankResponse;
		}

		// set the partition and row key based on the provided attributes
		updatedBook.PartitionKey = genre;
		updatedBook.RowKey = id;

		try {
			await _tableClient.UpdateEntityAsync(updatedBook, ETag.All, TableUpdateMode.Replace);
			var goodResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await goodResponse.WriteStringAsync("message: book updated successfully");
			return goodResponse;
		} catch (RequestFailedException ex) when(ex.Status == 404) {
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("error: item not found");
			return notFoundResponse;
		} catch (Exception e) {
			var genericErrorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
			await genericErrorResponse.WriteStringAsync("error: server exploded");
			return genericErrorResponse;
		}
	}
	[Function("DeleteBook")]
	public async Task<HttpResponseData> DeleteBook([HttpTrigger(AuthorizationLevel.Anonymous, "delete",
	Route = "books/{genre}/{id}")] HttpRequestData req, string genre, string id) {
		try {
			await _tableClient.DeleteEntityAsync(genre, id);
			var goodResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await goodResponse.WriteStringAsync("message: delete successfully");
			return goodResponse;
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("error: item not found");
			return notFoundResponse;
		}
		catch (Exception e)
		{
			var genericErrorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
			await genericErrorResponse.WriteStringAsync("error: server exploded");
			return genericErrorResponse;
		}
	}
}