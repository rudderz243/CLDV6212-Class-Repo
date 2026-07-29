using FunctionCRUDG2.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FunctionCRUDG2.Services
{
	internal class functionService
	{
		private readonly HttpClient _httpClient;
		// function URL, be sure to update to match yours! :)
		private const string functionURL = "http://localhost:7017/api/items";
		// create a single copy of the API service class that we will call everywhere else
		// this is called a singleton - it is a type of software pattern
		private static readonly Lazy<functionService> _instance = new(() => new functionService());
		public static functionService Instance = _instance.Value;

		// constructor - same name as the class
		private functionService() {
			_httpClient = new();
		}

		// Get all items from the function (R)
		public async Task<List<MenuItem>> GetAllItemsAsync() {
			try {
				// send a request and wait for a response
				var response = await _httpClient.GetAsync(functionURL);
				// ensure that the communication was successful, otherwise we throw an exception
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				var allItems = JsonSerializer.Deserialize<List<MenuItem>>(content);
				return allItems;
			} catch (Exception ex) {
				Console.WriteLine("Error occurred getting items: " + ex.Message);
				return new List<MenuItem>();
			}
		}
		// allow the user to search for a specific item (R)
		public async Task<MenuItem> GetSingleItem(int itemId) {
			try {
				var response = await _httpClient.GetAsync(functionURL + "?id=" + itemId);
				if (!response.IsSuccessStatusCode) {
					return null;
				} else {
					var content = await response.Content.ReadAsStringAsync();
					var decodedItem = JsonSerializer.Deserialize<MenuItem>(content);
					return decodedItem;
				}

			} catch (Exception ex) {
				Console.WriteLine("Error occurred getting items: " + ex.Message);
				return null;
			}
		}

		// Create a new item on the function (C)
		public async Task<MenuItem> CreateNewItemAsync(MenuItem itemToAdd) {
			try {
				// when adding we use a POST request
				var response = await _httpClient.PostAsJsonAsync(functionURL, itemToAdd);
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				return JsonSerializer.Deserialize<MenuItem>(content);
			} catch (Exception ex) {
				Console.WriteLine("Error adding new item: " + ex.Message);
				return null;
			}
		}

		public async Task<MenuItem> UpdateItemAsync(MenuItem itemToUpdate) {
			try {
				var response = await _httpClient.PutAsJsonAsync(functionURL, itemToUpdate);
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				var decodedResponse = JsonSerializer.Deserialize<MenuItem>(content);
				return decodedResponse;
			} catch (Exception ex) {
				Console.WriteLine("Error updating: " + ex.Message);
				return null;
			}
		}

		// delete an existing item (D)
		public async Task<MenuItem> DeleteItemAsync(int id) {
			try {
				var response = await _httpClient.DeleteAsync(functionURL + "?id=" + id);
				if (!response.IsSuccessStatusCode) {
					return null;
				} else {
					var content = await response.Content.ReadAsStringAsync();
					var deletedItem = JsonSerializer.Deserialize<MenuItem>(content);
					return deletedItem;
				}
			} catch (Exception ex) {
				Console.WriteLine("Error deleting: " + ex.Message);
				return null;
			}
		}


	}
}
