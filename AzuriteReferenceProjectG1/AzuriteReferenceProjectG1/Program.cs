using AzuriteReferenceProjectG1.Models;
using AzuriteReferenceProjectG1.Services;

namespace AzuriteReferenceProjectG1
{
	internal class Program
	{
		private const string ConnectionString = "UseDevelopmentStorage=True;";
		private const string QueueName = "ref-queue";
		private const string TableName = "ref-table";
		private const string BlobName = "ref-blob";

		// declare singletons for each of our azure storage services
		private static readonly QueueService _q = new QueueService(ConnectionString, QueueName);
		private static readonly TableService _table = new TableService(ConnectionString, TableName);
		private static readonly BlobService _blob = new BlobService(ConnectionString, BlobName);

		public static async Task Main(string[] args)
		{
			Console.WriteLine("Azurite Reference Console App");
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

				try 
				{
					switch (userChoice)
					{
						// Add Queue Item
						case "1":
							await QueueItemHandler();
							break;
						// Peek Queue Item
						case "2":
							await PeekItemHandler();
							break;
						// Add Item To Table
						case "3":
							await TableAddHandler();
							break;
						// View All Table Items
						case "4":
							await TableGetHandler();
							break;
						// UPload Blob Item
						case "5":
							await UploadBlobHandler();
							break;
						// DOWNload Blob Item
						case "6":
							await DownloadBlobHandler();
							break;
						// View All Blob Items
						case "7":
							await ListBlobHandler();
							break;
						// Exit
						case "8":
							keepRunning = false;
							break;
						// No Match
						default:
							Console.WriteLine("Invalid Option, Try Again.");
							break;

					}
				} catch (Exception ex)
				{
					Console.WriteLine("An Error Occured: " + ex.Message);
				}

			}
		}

		private static async Task QueueItemHandler()
		{
			Console.WriteLine(); // blank to put it on a new line
			// get the name of the customer
			Console.Write("Enter Name: ");
			string name = Console.ReadLine();
			// get the action
			Console.Write("Enter Action: ");
			string action = Console.ReadLine();
			// create object
			var eventToLog = new CustomerLog
			{
				CustomerName = name,
				Action = action
			};
			// q the action
			await _q.EnqueueMessageAsync(eventToLog);
			Console.WriteLine("Item Added To Queue");
		}

		private static async Task PeekItemHandler()
		{
			Console.WriteLine();
			// get the message using our singleton
			string? message = await _q.PeekMessageAsync();

			if (message is null) 
			{
				Console.WriteLine("There Is No Item To Peek.");
				return;
			}
			Console.WriteLine("Peeked Message: " + message);
		}

		private static async Task TableAddHandler()
		{
			Console.WriteLine();
			// get the name of the customer
			Console.Write("Enter Name: ");
			string name = Console.ReadLine();
			// get the action
			Console.Write("Enter Action: ");
			string action = Console.ReadLine();
			// create object
			var eventToLog = new CustomerLog
			{
				CustomerName = name,
				Action = action
			};
			// add to table
			await _table.AddEntityAsync(eventToLog);
			Console.WriteLine("Item Added To Table");
		}

		private static async Task TableGetHandler()
		{
			Console.WriteLine();

			List<CustomerLog> tableItems = await _table.GetAllEntitiesAsync();
			
			if (tableItems.Count is 0)
			{
				Console.WriteLine("No Items In Table");
				return;
			}

			foreach (var item in tableItems)
			{
				Console.WriteLine(item.ToString());
			}
		}

		private static async Task DownloadBlobHandler()
		{
			Console.WriteLine();

			Console.Write("Enter File Name To Download: ");
			string fileName = Console.ReadLine();

			Console.Write("Enter File Path To Save Item: ");
			string downloadPath = Console.ReadLine();

			await _blob.DownloadFileAsync(fileName, downloadPath);
			Console.WriteLine("File Downloaded Successfully");
		}

		private static async Task UploadBlobHandler()
		{
			Console.WriteLine();

			Console.Write("Enter File Name To Upload: ");
			string fileName = Console.ReadLine();

			Console.Write("Enter File Path To Upload From: ");
			string uploadPath = Console.ReadLine();

			await _blob.UploadFileAsync(uploadPath, fileName);
			Console.WriteLine("File Uploaded Successfully");
		}

		private static async Task ListBlobHandler() 
		{
			Console.WriteLine();

			var fileNames = new List<string>();
			fileNames = await _blob.ListAllAsync();

			if (fileNames.Count is 0)
			{
				Console.WriteLine("No Files In Blob");
			}

			foreach(var file in fileNames)
			{
				Console.WriteLine("# " + file);
			}
		}
	}
}
