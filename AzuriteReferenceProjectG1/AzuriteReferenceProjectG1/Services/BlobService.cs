using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AzuriteReferenceProjectG1.Services
{
	public class BlobService
	{
		// the blob client will interact with the Azurite blob
		private readonly BlobContainerClient _blobClient;

		// Constructor -> initialize
		public BlobService(string connectionString, string blobName) {
			_blobClient = new BlobContainerClient(connectionString, blobName);
			_blobClient.CreateIfNotExists();
		}

		// Upload -> add file into the blob container
		public async Task UploadFileAsync(string filePath, string blobName)
		{
			// 1: get the blob ready for the upload
			BlobClient blob = _blobClient.GetBlobClient(blobName);
			// 2: convert the file into a stream, and then begin the upload process
			using FileStream stream = File.OpenRead(filePath);
			// 3: stream the bytes into the blob container (replacing anything with the same name)
			await blob.UploadAsync(stream, overwrite: true);
		}

		// Download -> get a specific file from the blob
		public async Task DownloadFileAsync(string blobName, string downloadDestination)
		{
			// 1: get the file
			BlobClient blob = _blobClient.GetBlobClient(blobName);
			// 2: save the file
			await blob.DownloadToAsync(downloadDestination);
		}

		// ListAll -> return the names of the files in the Blob container
		public async Task<List<string>> ListAllAsync()
		{
			// declare an empty list to hold the files
			var fileNames = new List<string>();
			// loop through every item in the container
			await foreach (var item in _blobClient.GetBlobsAsync())
			{
				// add each items name to teh list
				fileNames.Add(item.Name);
			}
			// return the list (if no items, empty list will be returned instead)
			return fileNames;
		}
	}
}
