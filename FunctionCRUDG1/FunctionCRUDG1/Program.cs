using FunctionCRUDG1.Models;
using FunctionCRUDG1.Services;
using System.Diagnostics;

namespace FunctionCRUDG1
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// call in and create the singleton for our API service
			var service = functionService.Instance;

			var running = true;

			while (running) {
				Console.WriteLine("-- Restaurant Function Manager --");
				Console.WriteLine("1) View ALL items");
				Console.WriteLine("2) Search using ID");
				Console.WriteLine("3) Create NEW item");
				Console.WriteLine("4) Replace existing item");
				Console.WriteLine("5) Delete existing item");
				Console.WriteLine("6) Exit");

				string userChoice = Console.ReadLine();
				Console.WriteLine();

				switch (userChoice) {
					case "1":
						await GetAllItemsAsync(service);
						break;
					case "2":
						await SearchForItemAsync(service);
						break;
					case "3":
						await CreateNewItemAsync(service);
						break;
					case "4":
						await ReplaceExistingItem(service);
						break;
					case "5":
						await DeleteItemAsync(service);
						break;
					case "6":
						Console.WriteLine("Exiting...");
						running = false;
						continue;
					default:
						Console.WriteLine("Invalid option entered.");
						break;
				}
			}
		}

		// uses our function service class we created to get a list of all the items
		private static async Task GetAllItemsAsync(functionService f) {
			Console.WriteLine("\n -- Getting Items --");

			var items = await f.GetAllItemsAsync();

			if (items == null || items.Count == 0) {
				Console.WriteLine("There are currently no items stored in the function.");
				return;
			}

			foreach (var item in items) {
				Console.WriteLine("-------------------");
				Console.WriteLine("ID:\t" + item.id);
				Console.WriteLine("Name:\t" + item.itemName);
				Console.WriteLine("Price:\t" + item.itemPrice);
			}
		}

		// this helper method searches for a specific item using a user inputted ID
		private static async Task SearchForItemAsync(functionService f) {
			Console.WriteLine("\n -- Searching for Item --");

			Console.Write("Enter the item ID to search for > ");
			if (!int.TryParse(Console.ReadLine(), out int id)) {
				Console.WriteLine("Invalid ID was entered.");
				return;
			}

			var searchedItem = await f.GetSingleItemAsync(id);
			if (searchedItem == null) {
				Console.WriteLine("No item was found using id " + id);
			} else {
				Console.WriteLine("ID:\t" + searchedItem.id);
				Console.WriteLine("Name:\t" + searchedItem.itemName);
				Console.WriteLine("Price:\t" + searchedItem.itemPrice);
			}
		}

		// this helper function will make use of the function service class + user input to create a new item
		private static async Task CreateNewItemAsync(functionService f) {
			Console.WriteLine("\n -- Creating New Item --");

			Console.Write("Please enter the ID of the new item > ");
			if (!int.TryParse(Console.ReadLine(), out int id)) {
				Console.WriteLine("Invalid ID has been entered.");
				return;
			}

			Console.Write("Please enter the name of the new item > ");
			var name = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(name)) {
				Console.WriteLine("Invalid name has been entered.");
				return;
			}

			Console.Write("Please enter the price of the new item > ");
			if (!double.TryParse(Console.ReadLine(), out double price)) {
				Console.WriteLine("Invalid price has been entered.");
				return;
			}

			var newItemToCreate = new MenuItem { id = id, itemName = name, itemPrice = price };
			var response = await f.CreateItemAsync(newItemToCreate);

			if (response == null) {
				Console.WriteLine("Failed to create new item with id " + id);
				return;
			} else {
				Console.WriteLine("Successfully created new item with id " + id);
			}
		}

		// replace an existing item with new information
		private static async Task ReplaceExistingItem (functionService f) {
			Console.WriteLine("\n -- Replacing Existing Item --");

			Console.Write("Enter the ID of the item you wish to replace > ");
			if (!int.TryParse(Console.ReadLine(), out int id)) {
				Console.WriteLine("Invalid ID has been entered.");
				return;
			}

			Console.Write("Please enter the new name for the item > ");
			var name = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(name))
			{
				Console.WriteLine("Invalid name has been entered.");
				return;
			}

			Console.Write("Please enter the new price for the item > ");
			if (!double.TryParse(Console.ReadLine(), out double price))
			{
				Console.WriteLine("Invalid price has been entered.");
				return;
			}

			var itemToReplaceExisting = new MenuItem { id = id, itemName = name, itemPrice = price };
			var reponse = await f.ReplaceItemAsync(itemToReplaceExisting);

			if (reponse == null)
			{
				Console.WriteLine("Failed to replace item with id " + id);
				return;
			}
			else {
				Console.WriteLine("Successfully replaced item with id " + id);
				return;
			}
		}

		// the delete helper deletes an item
		private static async Task DeleteItemAsync(functionService f) {
			Console.WriteLine("\n -- Delete Existing Item --");

			Console.Write("Enter the ID of the item you wish to delete > ");
			if (!int.TryParse(Console.ReadLine(), out int id))
			{
				Console.WriteLine("Invalid ID has been entered.");
				return;
			}

			var deletedItem = await f.DeleteItemAsync(id);

			if (deletedItem == null)
			{
				Console.WriteLine("Failed to delete item with id " + id);
			}
			else {
				Console.WriteLine("Successfully deleted item with id " + id);
			}
		}
	}
}
