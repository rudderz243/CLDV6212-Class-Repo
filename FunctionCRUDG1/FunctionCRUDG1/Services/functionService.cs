using FunctionCRUDG1.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FunctionCRUDG1.Services
{
	public sealed class functionService
	{
		private readonly HttpClient _httpClient;
		// function URL - this would usually go into appsettings.json, however we are lazy and learning
		// this connection string would have to be updated to match YOUR function :)
		const string functionURL = "http://localhost:6769/api/items";
		// create a single instance of the functionService class, that we will call throughout our entire
		// program. this way, we can preserve server resources, by only having 1 single connection per client
		// this is called a SINGLETON - a single instance of an object
		private static readonly Lazy<functionService> _instance = new(() => new functionService());
		public static functionService Instance = _instance.Value;
		// constructor - we use this to prepare our http client for connections
		public functionService() {
			_httpClient = new HttpClient();
		}

		// connects to the function, and gets a list of all the menu items
		public async Task<List<MenuItem>> GetAllItemsAsync() {
			try {
				// send a request -> recieve a response
				var response = await _httpClient.GetAsync(functionURL);
				// we write this line to throw an EXCEPTION if something goes wrong
				response.EnsureSuccessStatusCode();

				// get the content that the API returned out of the response
				var content = await response.Content.ReadAsStringAsync();
				var listOfItems = JsonSerializer.Deserialize<List<MenuItem>>(content);
				// check if we were able to convert the response back from JSON to a list
				if (listOfItems == null) {
					return new List<MenuItem>();
				} else {
					return listOfItems;
				}
			} catch (Exception ex) {
				Console.WriteLine("Error getting ALL items: " + ex.Message);
				return new List<MenuItem>();
			}
		}
		// get a single item from the function, using its ID
		public async Task<MenuItem?> GetSingleItemAsync(int itemIdToGet) {
			try {
				var response = await _httpClient.GetAsync(functionURL + "?id=" + itemIdToGet);
				// if the request failed
				if (!response.IsSuccessStatusCode) {
					return null;
				}

				var content = await response.Content.ReadAsStringAsync();
				var decodedItem = JsonSerializer.Deserialize<MenuItem>(content);

				// if we fail to decode what the content has
				if (decodedItem == null) {
					return null;
				} else {
					return decodedItem;
				}
			} catch (Exception ex) {
				Console.WriteLine("Error getting item: " + ex.Message);
				return null;
			}
		}
		// we need a helper function to create a NEW instance of an object in the function
		public async Task<MenuItem?> CreateItemAsync(MenuItem itemToCreate) {
			try {
				// post request -> send data with teh intent of creating something NEW
				var response = await _httpClient.PostAsJsonAsync(functionURL, itemToCreate);
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				var newItemAdded = JsonSerializer.Deserialize<MenuItem>(content);

				if (newItemAdded == null) {
					return null;
				} else {
					return newItemAdded;
				}
			} catch (Exception ex) {
				Console.WriteLine("Error adding item: " + ex.Message) ;
				return null;
			}
		}

		// the PUT function replaces an already existing object
		public async Task<MenuItem?> ReplaceItemAsync(MenuItem itemToReplace) {
			try {
				// put request -> replaces an existing object with a new one
				var response = await _httpClient.PutAsJsonAsync(functionURL, itemToReplace);
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				var updatedObject = JsonSerializer.Deserialize<MenuItem>(content);

				if (updatedObject == null) {
					return null;
				} else {
					return updatedObject;
				}
			} catch (Exception ex) {
				Console.WriteLine("Error replacing object: " + ex.Message);
				return null;
			}
		}

		public async Task<MenuItem?> DeleteItemAsync(int itemToDelete) {
			try {
				var response = await _httpClient.DeleteAsync(functionURL + "?id=" + itemToDelete);

				if (!response.IsSuccessStatusCode) {
					return null;
				}

				var content = await response.Content.ReadAsStringAsync();
				var decodedItem = JsonSerializer.Deserialize<MenuItem>(content);

				if (decodedItem == null) {
					return null;
				} else {
					return decodedItem;
				}
			} catch (Exception ex) {
				Console.WriteLine("Error deleting: " + ex.Message);
				return null;
			}
		}
	}
}
