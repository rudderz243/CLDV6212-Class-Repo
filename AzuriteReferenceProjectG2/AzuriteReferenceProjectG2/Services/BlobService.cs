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
	}
}