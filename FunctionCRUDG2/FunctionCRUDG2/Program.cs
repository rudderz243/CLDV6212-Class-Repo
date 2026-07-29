using FunctionCRUDG2.Models;
using FunctionCRUDG2.Services;

namespace FunctionCRUDG2
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// call in an instance of our function class
			var func = functionService.Instance;

			bool running = true;

			while (running) {
				Console.WriteLine("Welcome to restaurant function manager.");
				Console.WriteLine("1) View ALL items");
				Console.WriteLine("2) Search by ID");
				Console.WriteLine("3) Add new item");
				Console.WriteLine("4) Update existing item");
				Console.WriteLine("5) Delete existing item");
				Console.WriteLine("6) Exit");

				string userChoice = Console.ReadLine();
				Console.WriteLine();

				switch (userChoice) {
					case "1":
						await ViewAllItemsAsync(func);
						break;
					case "2":
						await SearchForItemAsync(func);
						break;
					case "3":
						await CreateNewItemAsync(func);
						break;
					case "4":
						await ReplaceExistingItem(func);
						break;
					case "5":
						await DeleteItemAsync(func);
						break;
					case "6":
						running = false;
						Console.WriteLine("Exiting...");
						continue;
					default:
						Console.WriteLine("Please choose a valid option");
						break;
				}
			}
		}
		// using the function service class we wrote, get and display all items
		private static async Task ViewAllItemsAsync(functionService func) {
			Console.WriteLine("-- Getting Items --");
			var items = await func.GetAllItemsAsync();

			if (items == null || items.Count == 0) {
				Console.WriteLine("There are currently no items stored.");
				return;
			} else {
				foreach (var item in items)
				{
					Console.WriteLine("-------------------");
					Console.WriteLine("ID: \t" + item.id);
					Console.WriteLine("Name:\t"+ item.itemName);
					Console.WriteLine("Price:\tR" + item.itemPrice);
				}
			}
		}
		// this will use the function service to SEARCH for a specific item
		private static async Task SearchForItemAsync(functionService func) {
			Console.WriteLine("-- Searching for Item --");
			Console.Write("Enter item ID > ");

			// check for a valid int from the user
			if (int.TryParse(Console.ReadLine(), out int id)) {
				var item = await func.GetSingleItem(id);

				// null check the item (See if it exists)
				if (item == null) {
					Console.WriteLine("Item not found with ID " + id);
				}
				else
				{
					Console.WriteLine("ID: \t" + item.id);
					Console.WriteLine("Name:\t" + item.itemName);
					Console.WriteLine("Price:\tR" + item.itemPrice);
				}
			}
			else {
				Console.WriteLine("Invalid input. Please enter a numerical ID.");
			}
		}

		// this method will use the function service to create a new item based on user input
		public static async Task CreateNewItemAsync (functionService func) {
			Console.WriteLine("-- Creating New Item --");
			
			Console.Write("Please enter item ID > ");
			if (!int.TryParse(Console.ReadLine(), out int id)) {
				Console.WriteLine("Invalid ID entered.");
				return;
			}

			Console.Write("Please enter item name > ");
			var name = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(name)) {
				Console.WriteLine("Invalid name entered.");
				return;
			}

			Console.Write("Please enter item price > ");
			if (!double.TryParse(Console.ReadLine(), out double price)) {
				Console.WriteLine("Invalid price entered.");
				return;
			}

			var newItemToCreate = new MenuItem { id = id, itemName = name, itemPrice = price };
			var created = await func.CreateNewItemAsync(newItemToCreate);

			// if the item was created successfully
			if (created != null) {
				Console.WriteLine("Successfully created item with an ID of " + created.id);
			} else {
				Console.WriteLine("Failed to create item.");
			}
		}

		// replaces an existing item with a new one
		private static async Task ReplaceExistingItem(functionService func) {
			Console.WriteLine("-- Replacing Item --");
			
			Console.Write("Enter the ID of the item you wish to replace > ");
			if (!int.TryParse(Console.ReadLine(), out int id)) {
				Console.WriteLine("Invalid item ID entered.");
				return;
			}

			Console.Write("Please enter the name for the new item > ");
			var name = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(name)) {
				Console.WriteLine("Invalid name entered.");
				return;
			}

			Console.Write("Please enter the new price > ");
			if (!double.TryParse(Console.ReadLine(), out double price)) {
				Console.WriteLine("Invalid price entered.");
				return;
			}

			var itemToReplace = new MenuItem { id = id, itemName = name, itemPrice = price };
			var reponse = await func.UpdateItemAsync(itemToReplace);
			
			if (reponse != null) {
				Console.WriteLine("Successfully replaced item with ID " + id);
			} else {
				Console.WriteLine("Failed to update item with ID " + id);
			}
		}
		// delete lets us delete by specifying an ID
		private static async Task DeleteItemAsync(functionService func) {
			Console.WriteLine("-- Deleting Item --");

			Console.Write("Enter item ID to delete > ");
			if (int.TryParse(Console.ReadLine(), out int id))
			{
				{
					var deletedItem = await func.DeleteItemAsync(id);

					if (deletedItem != null)
					{
						Console.WriteLine("Item " + id + " deleted.");
					}
					else
					{
						Console.WriteLine("Item " + id + " not deleted.");
					}
				}
			} else {
				Console.WriteLine("Invalid ID entered.");
			}
		}
	}
}
