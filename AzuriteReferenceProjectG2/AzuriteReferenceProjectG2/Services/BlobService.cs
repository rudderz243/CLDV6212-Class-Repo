using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzuriteReferenceProjectG2.Services
{
	public class BlobService
	{
		// create the object to interact with Azurite
		private readonly BlobContainerClient _blob;

		public BlobService(string connectionString, string blobName)
		{
			// 1: initialize
			_blob = new BlobContainerClient(connectionString, blobName);
			// 2: create
			_blob.CreateIfNotExists();
		}
		
		// Upload -> add file into a blob container
		public async Task UploadFileAsync(string filePath, string fileName)
		{
			// 1: prepare the blob container for the upload
			BlobClient blob = _blob.GetBlobClient(fileName);
			// 2: convert the file into a stream, then begin uploading it
			using FileStream stream = File.OpenRead(filePath);
			// 3: stream the bytes of the file into the blob
			await blob.UploadAsync(stream, overwrite: true); // overwrite:true -> replace file with same name
		}

		// Download -> get a file from the Blob container
		public async Task DownloadFileAsync(string fileName, string filePath)
		{
			// 1: get the file from the blob container
			BlobClient blob = _blob.GetBlobClient(fileName);
			// 2: save the file
			await blob.DownloadToAsync(filePath);
		}

		// ListAll -> return a list of names of files in the Blob we can download
		public async Task<List<string>> ListAllAsync()
		{
			// declare an empty list to store the names of each of the files
			var fileNames = new List<string>;

			// loop through each and every item in the blob and add the name to the list
			await foreach (var item in _blob.GetBlobsAsync())
			{
				// add the name into the list
				fileNames.Add(item.Name);
			}
			// return the list (empty if no files; names if files are present)
			return fileNames;
		}
	}
}