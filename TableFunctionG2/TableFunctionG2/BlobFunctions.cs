using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TableFunctionG2
{
	public class BlobFunctions
	{
		// prepare our class to make use of the singleton declared in Program.cs
		private readonly BlobContainerClient _blobClient;

		public BlobFunctions(BlobServiceClient blobService)
		{
		// wrap everything in try{} catch{}
		// check for the correct file types
		// return 400 for any user errors (wrong file, missing file, etc)
		// return 500 for any server errors (cannot connect to azurite, upload failed, etc).

			// open the specific container we are looking for
			_blobClient = blobService.GetBlobContainerClient("example-blob");
			// if it wasn't found, create it
			_blobClient.CreateIfNotExists();
		}

		[Function("UploadBlob")]
		public async Task<HttpResponseData> Upload([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="blobs/{filename}")]
		HttpRequestData req, string filename)
		{
			// try and find if the file already exists
			BlobClient blob = _blobClient.GetBlobClient(filename);
			// if it exists: overwrite, otherwise, upload the file
			await blob.UploadAsync(req.Body, overwrite: true);

			var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await response.WriteStringAsync("blob uploaded successfully");
			return response;
		}
		[Function("DownloadBlob")]
		public async Task<HttpResponseData> Download ([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="blobs/{filename}")]
		HttpRequestData req, string filename) {
			// try and get the file
			BlobClient blob = _blobClient.GetBlobClient(filename);

			// if file does NOT exist
			if (!await blob.ExistsAsync()) {
				// 404
				return req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			}
			// otherwise write the file out
			var stream = await blob.OpenReadAsync();
			var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
			// set the response TYPE using headers to indicate a file is coming instead of text/json
			response.Headers.Add("Content-Type", "application/octet-stream");
			await stream.CopyToAsync(response.Body);
			return response;
		}
		[Function("ListBlob")]
		public async Task<HttpResponseData> ListAll([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="blobs")]
		HttpRequestData req) {
			// create an empty list to store the information
			// object -> because it contains files
			var fileList = new List<object>();

			// fetch every file in the blob storage
			await foreach (var item in _blobClient.GetBlobsAsync()) {
				// and add the relevant information to the list
				fileList.Add(new
				{
					FileName = item.Name,
					SizeBytes = item.Properties.ContentLength,
					DateModified = item.Properties.LastModified
				});
			}
			// once we have got the information for all the files, we return to the user
			var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await response.WriteAsJsonAsync(fileList);
			return response;
		}
	}
}
