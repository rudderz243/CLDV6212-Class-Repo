using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
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

		if (book == null)
		{
			var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest); // (code 400)
			await badResponse.WriteStringAsync("Invalid book information provided.");
			return badResponse;
		}

		// if required data is missing: return 400
		if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrEmpty(book.Author))
		{
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
	Route="books/{genre}")] HttpRequestData req, string genre)
	{
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

	[Function("GetBookById")]
	public async Task<HttpResponseData> GetBookById([HttpTrigger(AuthorizationLevel.Anonymous, "get",
	Route = "books/{genre}/{id}")] HttpRequestData req, string genre, string id)
	{
		try
		{
			// to get an item from TableStorage, you need to provide both the paritionKey and the rowKey
			var requestedEntity = await _tableClient.GetEntityAsync<BookEntity>(genre, id);
			// if we are able to find it (no errors), we return it
			var goodResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await goodResponse.WriteAsJsonAsync(requestedEntity);
			return goodResponse;
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			// when things aren't found, we return a 404 to the user/client/requesting service
			var badResponse = req.CreateResponse(HttpStatusCode.NotFound);
			await badResponse.WriteStringAsync($"error: item not found with id {id}");
			return badResponse;
		}
		catch (Exception ex)
		{
			// return 500
			var genericResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
			await genericResponse.WriteStringAsync("error: server explod");
			return genericResponse;
		}
	}

	[Function("UpdateBook")]
	public async Task<HttpResponseData> UpdateBook([HttpTrigger(AuthorizationLevel.Anonymous, "put",
	Route = "books/{genre}/{id}")] HttpRequestData req, string genre, string id)
	{
		// get update book information from the req body
		var updatedBook = await JsonSerializer.DeserializeAsync<BookEntity>(req.Body);

		if (updatedBook == null)
		{
			var blankResponse = req.CreateResponse(HttpStatusCode.BadRequest);
			await blankResponse.WriteStringAsync("error: invalid data provided");
			return blankResponse;
		}
		// set the partition and the row keys based on the users request
		updatedBook.PartitionKey = genre;
		updatedBook.RowKey = id;

		try
		{
			// using the new info, update EVERYTHING, and replace the existing entity
			await _tableClient.UpdateEntityAsync(updatedBook, ETag.All, TableUpdateMode.Replace);
			var goodResponse = req.CreateResponse(HttpStatusCode.OK);
			await goodResponse.WriteStringAsync("message: book successfully updated");
			return goodResponse;
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			// when things aren't found, we return a 404 to the user/client/requesting service
			var badResponse = req.CreateResponse(HttpStatusCode.NotFound);
			await badResponse.WriteStringAsync($"error: item not found with id {id}");
			return badResponse;

		}
	}

	[Function("DeleteBook")]
	public async Task<HttpResponseData> DeleteBook([HttpTrigger(AuthorizationLevel.Anonymous, "delete",
	Route = "books/{genre}/{id}")] HttpRequestData req, string genre, string id) {
		try {
			await _tableClient.DeleteEntityAsync(genre, id);
			var response = req.CreateResponse(HttpStatusCode.NoContent);
			return response;
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			// when things aren't found, we return a 404 to the user/client/requesting service
			var badResponse = req.CreateResponse(HttpStatusCode.NotFound);
			await badResponse.WriteStringAsync($"error: item not found with id {id}");
			return badResponse;

		}


	}
}