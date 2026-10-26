using LibraryGatewayAPI.DTOs;
using System.Net.Http.Headers;

namespace LibraryGatewayAPI.Clients
{
	public class DocumentBlobClient
	{
		private readonly HttpClient _http;

		public DocumentBlobClient(HttpClient http) 
		{
			_http = http;
		}

		public async Task<IEnumerable<BlobMetadataDto>> ListLoansAsync()
		{
			var response = await _http.GetAsync("api/blobs");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<IEnumerable<BlobMetadataDto>>();
		}
						// stream -> the file itself
		public async Task<(Stream content, string contentType)> DownloadLoanAsync(string fileName)
		{
			var response = await _http.GetAsync($"api/blobs/{Uri.EscapeDataString(fileName)}");
			response.EnsureSuccessStatusCode();

			// need to prep the file to be downloaded from the API
			var contentType = string.Empty;
			if (response.Content.Headers.ContentType?.MediaType != null) 
			{
				// if the file has its type set (and we get it in the response from the function), use that
				contentType = response.Content.Headers.ContentType?.MediaType;
			} else 
			{
				// otherwise, just use the default octet-stream to indicate it is a file being sent (not json or text)
				contentType = "application/octet-stream";
			}
			// now that we've set WHAT the content is, we get the actual content
			var streamOfContent = await response.Content.ReadAsStreamAsync();
			// and return both
			return (streamOfContent, contentType);
		}
										// name of file, the actual file, and what it is
		public async Task UploadLoanAsync(string fileName, Stream content, string contentType)
		{
			// going from a raw byte stream to a file
			using var contentStream = new StreamContent(content);
			// set the contentType header to send to the function
			contentStream.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			// send it off to the function
			var response = await _http.PostAsync($"api/blobs/${Uri.EscapeDataString(fileName)}", contentStream);
			response.EnsureSuccessStatusCode();
		}
	}
}
