using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RestaurantFunctionG2.Models;
using System.Text.Json;
namespace RestaurantFunctionG2;

public class Function1
{
	List<MenuItem> menuItems = new(); // we are using a list because databases = effort

	private readonly ILogger<Function1> _logger;

	public Function1(ILogger<Function1> logger)
	{
		_logger = logger;
	}

	[Function("items")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous,
	"get", "post", "put", "delete")] HttpRequestData req) // adding "put" and "delete" allows our func to do new things
	{
		_logger.LogInformation("C# HTTP trigger function processed a request.");
		// by default, we say everything is OK, this will change if we encounter an error
		var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "application/json");

		// what does the sender/client want us to do?
		var method = req.Method;

		switch (method)
		{
			// GET -> user wants to get/retrieve information from us
			case "GET":
				// does the user want to get a SPECIFIC item? or a list of all of them
				var requestedId = System.Web.HttpUtility.ParseQueryString(req.Url.Query)["id"];
				var isRequesting = string.IsNullOrEmpty(requestedId);

				if (isRequesting)
				{
					// find the specific item that they want
					var requestedItem = menuItems.FirstOrDefault(i => i.id == int.Parse(requestedId));
					// we then check if that item actually exists
					if (requestedItem != null)
					{
						// return the item they want :)
						await response.WriteStringAsync(JsonSerializer.Serialize(requestedItem));
						return response;
					}
					else
					{
						// return an error if it was not found :(
						response.StatusCode = System.Net.HttpStatusCode.NotFound;
						return response;
					}
				}
				else
				{ // if the user DID NOT specify an ID
					await response.WriteStringAsync(JsonSerializer.Serialize(menuItems));
					return response;
				}
			// POST -> user wants to add/post new information to us
			case "POST":
				var postedDataPost = await new StreamReader(req.Body).ReadToEndAsync();
				var newItemToAdd = JsonSerializer.Deserialize<MenuItem>(postedDataPost);
				// if we were able to understand the user input, and it returned a valid object
				if (newItemToAdd != null)
				{
					// add the item to the list
					menuItems.Add(newItemToAdd);
					response.StatusCode = System.Net.HttpStatusCode.Created; // created == 201
					await response.WriteStringAsync(JsonSerializer.Serialize(newItemToAdd));
					return response;
				}
				else
				{
					response.StatusCode = System.Net.HttpStatusCode.BadRequest;
					return response;
				}
			// PUT -> the user wants to put something in the place of something else (replace something)
			case "PUT":
				var postedDataPut = await new StreamReader(req.Body).ReadToEndAsync();
				var itemToReplace = JsonSerializer.Deserialize<MenuItem>(postedDataPut);
				// if the user did not provide valid information, yell at them
				if (itemToReplace == null)
				{
					response.StatusCode = System.Net.HttpStatusCode.BadRequest;
					return response;
				}
				var existingObjectToReplace = menuItems.FirstOrDefault(i => i.id == itemToReplace.id);
				// if the object they want to replace does not exist in the list, return a 404
				if (existingObjectToReplace == null)
				{
					response.StatusCode = System.Net.HttpStatusCode.NotFound; // not found == 404
					return response;
				}
				else
				{
					menuItems.Remove(existingObjectToReplace);
					menuItems.Add(itemToReplace);
					await response.WriteStringAsync(JsonSerializer.Serialize(itemToReplace));
					return response;
				}
			// DELETE -> the user wants to remove/delete something
			case "DELETE":
				var requestedIdToDelete = System.Web.HttpUtility.ParseQueryString(req.Url.Query)["id"];
				// if they don't give us an ID to delete, they are stupid >:(
				if (string.IsNullOrEmpty(requestedIdToDelete))
				{
					response.StatusCode = System.Net.HttpStatusCode.BadRequest;
					return response;
				}
				// find the item they want to delete
				var itemToDelete = menuItems.FirstOrDefault(i => i.id == int.Parse(requestedIdToDelete));

				// if we cannot find an item with the ID that they have specified, 404
				if (itemToDelete == null)
				{
					response.StatusCode = System.Net.HttpStatusCode.NotFound; // not found == 404
					return response;
				}
				else // otherwise delete the item
				{
					menuItems.Remove(itemToDelete);
					await response.WriteStringAsync(JsonSerializer.Serialize(itemToDelete));
					return response;
				}
			// default -> we don't care, try again
			default:
				response.StatusCode = System.Net.HttpStatusCode.MethodNotAllowed;
				return response;
		}
	}
}