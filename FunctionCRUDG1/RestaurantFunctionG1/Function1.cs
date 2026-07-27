using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RestaurantFunctionG1.Models;
using System.Text.Json;

namespace RestaurantFunctionG1;

public class Function1
{
	// create a list to "emulate" a database
	static List<MenuItem> menuItems = new();
		
	private readonly ILogger<Function1> _logger;

	public Function1(ILogger<Function1> logger)
	{
		_logger = logger;
	}

	[Function("items")]
	// by adding "put" and "delete", we are extending the functionality of our function to accept more request types
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete")] HttpRequestData req)
	{
		// crafting the skeleton for our responses
		// we start of by setting the response code to be 200 OK (which means the function has run succesfully)
		// we set the type of data we are going to be sending back to JSON data
		var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "application/json");

		// get the method the user used to trigger the function
		var method = req.Method;
		switch (method) {
			// a GET request is to request data from the function/api
			case "GET":
				// check whether the user has specified a specific ID that they want to get
				var requestedId = System.Web.HttpUtility.ParseQueryString(req.Url.Query)["id"];
				// we then store whether an ID was passed or not
				var idProvided = string.IsNullOrEmpty(requestedId);
				// if the user has provided an ID, we only give them the item that they have requested
				if (!idProvided) {
					// using LINQ to find the requested item
					var item = menuItems.FirstOrDefault(i => i.id == int.Parse(requestedId));
					// respond with the item if it was found
					if (item == null) response.StatusCode = System.Net.HttpStatusCode.NotFound;
					await response.WriteStringAsync(JsonSerializer.Serialize(item));
					return response;
				} else {
					// if no ID was provided, we just return the entire list
					await response.WriteStringAsync(JsonSerializer.Serialize(menuItems));
					return response;
				}
			// a POST request is to send new data to the function/api
			case "POST":
				var bodyDataPostRequest = await new StreamReader(req.Body).ReadToEndAsync();
				var newItemToAdd = JsonSerializer.Deserialize<MenuItem>(bodyDataPostRequest);

				// before we can add the new item, we must check that we actually have data to add
				if (newItemToAdd == null)
				{
					response.StatusCode = System.Net.HttpStatusCode.BadRequest; // 400 == user made an error
					return response;
				}
				else
				{
					menuItems.Add(newItemToAdd);
					response.StatusCode = System.Net.HttpStatusCode.Created; // 201 == item created succesfully
					await response.WriteStringAsync(JsonSerializer.Serialize(newItemToAdd));
					return response;
				}
			// a PUT request is to update existing data on the function/api
			case "PUT":
				var requestedIdToPut = System.Web.HttpUtility.ParseQueryString(req.Url.Query)["id"];

				if (string.IsNullOrEmpty(requestedIdToPut)) {
					response.StatusCode = System.Net.HttpStatusCode.BadRequest; // 400 == user made an error
					return response;
				}

				// check whether the item we are updating/replace does actually exist
				var exitingItemToReplace = menuItems.FirstOrDefault(i => i.id == int.Parse(requestedIdToPut));

				if (exitingItemToReplace == null) {
					response.StatusCode = System.Net.HttpStatusCode.NotFound; // 404 == item was not found
					return response;
				} else {
					var newDataToReplace = await JsonSerializer.DeserializeAsync<MenuItem>(req.Body);
					if (newDataToReplace == null) {
						response.StatusCode = System.Net.HttpStatusCode.BadRequest; // 400 == user made an error
						return response;
					} else {
						menuItems.Remove(exitingItemToReplace);
						menuItems.Add(newDataToReplace);
						await response.WriteStringAsync(JsonSerializer.Serialize(newDataToReplace));
						return response;
					}
				}
			// a DELETE request is to remove existing data from teh function/api
			case "DELETE":
				var requestedIdToDelete = System.Web.HttpUtility.ParseQueryString(req.Url.Query)["id"];

				if (requestedIdToDelete == null) {
					response.StatusCode = System.Net.HttpStatusCode.BadRequest;
					return response;
				}

				// find the object to delete
				var itemToDelete = menuItems.FirstOrDefault(i => i.id == int.Parse(requestedIdToDelete));

				if (itemToDelete == null) {
					response.StatusCode = System.Net.HttpStatusCode.NotFound;
					return response;
				} else {
					menuItems.Remove(itemToDelete);
					await response.WriteStringAsync(JsonSerializer.Serialize(itemToDelete));
					return response;
				}
			// if the method is not "get", "post", "put" or "delete"
			default:
				response.StatusCode = System.Net.HttpStatusCode.MethodNotAllowed;
				return response;
		}

	}
}