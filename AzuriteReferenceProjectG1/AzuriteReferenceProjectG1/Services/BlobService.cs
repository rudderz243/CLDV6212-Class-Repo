using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
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
	}
}
