using Azure.Core;
using AzuriteReferenceProjectG2.Models;
using AzuriteReferenceProjectG2.Services;

namespace AzuriteReferenceProjectG2
{
	internal class Program
	{
		private const string connectionString = "UseDevelopmentStorage=True;";
		private const string queueName = "ref-queue";
		private const string tableName = "ref-table";
		private const string blobName = "ref-blob";

		// singletons -> single instance of an object used to interface with a service
		private static readonly QueueService _q = new QueueService(connectionString, queueName);
		private static readonly TableService _t = new TableService(connectionString, tableName);
		private static readonly BlobService _b = new BlobService(connectionString, blobName);

		public static async Task Main(string[] args)
		{
			Console.WriteLine("== Azurite Reference Project ==");
			// loop control variable -> if false close app
			bool keepRunning = true;

			while (keepRunning)
			{
				Console.WriteLine(); // blank for spacing
				Console.WriteLine("1) Queue New Item");
				Console.WriteLine("2) Peek At Next Item");
				Console.WriteLine("3) Add New Item To Table");
				Console.WriteLine("4) View All Table Items");
				Console.WriteLine("5) Upload File To Blob");
				Console.WriteLine("6) Download File From Blob");
				Console.WriteLine("7) View All Files In Blob");
				Console.WriteLine("8) Exit");
				Console.Write("> ");
				string? userChoice = Console.ReadLine();

				// wrap everything in try {} catch {}
				try
				{
					switch (userChoice)
					{
						case "1":
							await QueueAddHandler();
							break;
						case "2":
							await QueuePeekHandler();
							break;
						case "3":
							await TableAddHandler();
							break;
						case "4":
							await TableGetHandler();
							break;
						case "5":
							await BlobUploadHandler();
							break;
						case "6":
							await BlobDownloadHandler();
							break;
						case "7":
							await BlobListHandler();
							break;
						case "8":
							keepRunning = false;
							break;
						default:
							Console.WriteLine("Invalid option - Please enter 1-8");
							break;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception occured: " + ex.Message);
				}

			}
		}

		// helper method -> handle adding to queue
		private static async Task QueueAddHandler()
		{
			// get in the information we want to Q
			Console.WriteLine("");
			Console.Write("Please enter the customer name: ");
			string name = Console.ReadLine();

			Console.Write("Please enter the customer action: ");
			string action = Console.ReadLine();

			var itemToQueue = new CustomerLog
			{
				CustomerName = name,
				Action = action
			};

			// q the item
			await _q.EnqueueMessageAsync(itemToQueue);
			Console.WriteLine("Item added to queue.");
		}

		// helper method -> get next item in queue
		private static async Task QueuePeekHandler()
		{
			Console.WriteLine();

			// get the message using our singleton
			string? message = await _q.PeekMessageAsync();

			// print the message (or an error if its null)
			if (message is null)
			{
				Console.WriteLine("No message to peek");
				return; // go back to the menu
			}
			else 
			{
				Console.WriteLine("Peeked message: " + message);
			}
		}

		// helper method -> add item to the table
		private static async Task TableAddHandler()
		{
			Console.WriteLine();

			// get the information to add
			Console.Write("Enter customer name: ");
			string name = Console.ReadLine();
			Console.Write("Enter customer action: ");
			string action = Console.ReadLine();

			// create object
			var itemToAdd = new CustomerLog
			{
				CustomerName = name,
				Action = action
			};

			// add to table
			await _t.AddEntityAsync(itemToAdd);
			Console.WriteLine("Item added to table");

		}
		// helper method -> get items from table
		private static async Task TableGetHandler()
		{
			Console.WriteLine();

			// store all items in a list
			var tableItems = new List<CustomerLog>();
			tableItems = await _t.GetAllEntitiesAsync();

			// check if there are items
			if (tableItems.Count is 0)
			{
				Console.WriteLine("No items in table");
				return;
			}
			else 
			{
				foreach (var item in tableItems)
				{
					Console.WriteLine(item.ToString());
				}
			}
		}

		// helper method -> upload item to a blob
		private static async Task BlobUploadHandler()
		{
			Console.WriteLine();

			// load the file from a specified folder
			Console.WriteLine("Enter the path to the file you would like to upload: ");
			string uploadFrom = Console.ReadLine();

			Console.WriteLine("What would you like to call the file in the blob container? ");
			string fileName = Console.ReadLine();

			await _b.UploadFileAsync(uploadFrom, fileName);
			Console.WriteLine("File uploaded successfully.");
		}

		// helper method -> download a file from the blob container to a place on your PC
		private static async Task BlobDownloadHandler()
		{
			Console.WriteLine();

			Console.WriteLine("Enter the name of the file you want to download from the container:");
			string fileName = Console.ReadLine();

			Console.WriteLine("Enter the location on your PC you want to save to:");
			string downloadTo = Console.ReadLine();

			await _b.DownloadFileAsync(fileName, downloadTo);
			Console.WriteLine("Downloaded file to " + downloadTo);
		}

		// helper method -> interact with _b to get a list of all possible files to download
		private static async Task BlobListHandler() 
		{
			Console.WriteLine();

			var fileNames = new List<string>();
			fileNames = await _b.ListAllAsync();

			if (fileNames.Count is 0) // is means the same things as ==
			{
				Console.WriteLine("No files are currently in the blob");
			}
			else
			{
				// loop through the list and print each file name
				foreach (var item in fileNames)
				{
					Console.WriteLine("# " + item);
				}
			}
		}
	}
}
