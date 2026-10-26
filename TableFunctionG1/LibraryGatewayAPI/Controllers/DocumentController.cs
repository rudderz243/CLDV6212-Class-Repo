using LibraryGatewayAPI.Clients;
using LibraryGatewayAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryGatewayAPI.Controllers
{
	[Route("api/v1/blobs")]
	[ApiController]
	[Produces("application/json")]
	public class DocumentController : ControllerBase
	{
		private readonly DocumentBlobClient _docs;

		public DocumentController(DocumentBlobClient docs)
		{
			_docs = docs;
		}

		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<BlobMetadataDto>), StatusCodes.Status200OK)] // list of files + 200 OK
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
		public async Task<IActionResult> ListFiles()
		{
			var result = await _docs.ListLoansAsync();
			return Ok(result);
		}

		[HttpGet("{filename}")]
		[Produces("application/octet-stream")] // instead of returning text, we return a file binary
		[ProducesResponseType(StatusCodes.Status200OK)] // file exists
		[ProducesResponseType(StatusCodes.Status404NotFound)] // file doesn't exist
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // server kaboom
		public async Task<IActionResult> Download([FromRoute] string filename)
		{
			var (content, type) = await _docs.DownloadLoanAsync(filename);
			return File(content, type);
		}

		[HttpPost("{filename}")]
		[Consumes("multipart/form-data", "application/octet-stream")] // takes IN these values
		[ProducesResponseType(StatusCodes.Status202Accepted)] // function accepted the upload request and it is in progress
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)] // file missing / not provided
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] 
		public async Task<IActionResult> Upload([FromRoute] string filename, IFormFile fileToSave)
		{
			if (fileToSave is null || fileToSave.Length is 0)
			{
				return BadRequest(new ErrorResponseDto { StatusCode = 400, Message = "File payload missing or empty" });
			}

			using var streamOfContent = fileToSave.OpenReadStream();
			await _docs.UploadLoanAsync(filename, streamOfContent, fileToSave.ContentType);
			return Accepted(new { message = "Document sent to Function to process upload" });
		}
	}
}
