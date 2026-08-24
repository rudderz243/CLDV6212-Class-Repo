using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableFunctionG1
{
	public class BlobFunctions
	{
		// prepare to use our singleton from Program.cs
		private readonly BlobContainerClient _blobClient;

		// use a constructor to initialize our container client
		public BlobFunctions(BlobServiceClient blobService) {
			// open this specific "folder" in the blob service
			_blobClient = blobService.GetBlobContainerClient("g1-blob");
			// if it doesn't already exist, create it
			_blobClient.CreateIfNotExists();
		}

		[Function("UploadBlob")]
		public async Task<HttpResponseData> Upload([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="blob/upload/{filename}")]
		HttpRequestData req, string filename) {
			// try {} catch {}
			// call in a request to the blob for this file
			// check firstly whether the file already exists in the blob
			BlobClient blob = _blobClient.GetBlobClient(filename);
			// if a file already exists with this name, we overwrite it
			await blob.UploadAsync(req.Body, overwrite: true);

			// if no file -> 400

			// if the file manages to upload successfully, 200 OK
			var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await response.WriteStringAsync($"file {filename} uploaded successfully.");
			return response;


			// catch -> response 500
		}

		[Function("DownloadBlob")]
		public async Task<HttpResponseData> Download([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="blob/download/{filename}")]
		HttpRequestData req, string filename) {
			// check if filename provided -> if not 400
			// try {} catch {}

			// get the file
			BlobClient blob = _blobClient.GetBlobClient(filename);

			// if file doesn't exist -> 404
			if (!await blob.ExistsAsync()) {
				return req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			}

			// we create a STREAM to send the data back to the client bit by bit
			var stream = await blob.OpenReadAsync();

			// first -> create response
			var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
			// second -> inform the user they are getting a FILE, not text/json
			response.Headers.Add("Content-Type", "application/octet-stream");
			// third -> stream the file
			await stream.CopyToAsync(response.Body);
			// finally -> once all the file has been sent, end the response
			return response;
		}

		[Function("ListBlobs")]
		public async Task<HttpResponseData> ListBlobs([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="blobs/list")]
		HttpRequestData req) {
			// try {} catch {}

			// create a blank list of type object to hold our information
			// object -> is not a fixed or defined type
			var fileList = new List<object>();

			// fetch every file in the blob to iterate over them
			await foreach (var item in _blobClient.GetBlobsAsync()) {
				// get the information we require
				fileList.Add(new
				{
					FileName = item.Name,
					FileSize = item.Properties.ContentLength,
					DateModified = item.Properties.LastModified
				});
			}

			// return the information to the user
			var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
			await response.WriteAsJsonAsync(fileList);
			return response;
		}
	}
}
