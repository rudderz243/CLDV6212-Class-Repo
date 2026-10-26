using LibraryGatewayAPI.Clients;
using LibraryGatewayAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryGatewayAPI.Controllers
{
	[Route("api/v1/books")]
	[ApiController]
	// every and all response from this controller will be in json
	[Produces("application/json")]
	public class BookController : ControllerBase
	{
		// call in our book client (the class we made that will communicate with the book functions)
		private readonly BooksTableClient _books;

		public BookController(BooksTableClient books)
		{
			_books = books;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)] // user provides bad data
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // the gateway api has an error
		public async Task<IActionResult> Create([FromBody] BookCreateRequestDto dto)
		{
			// get the information from the request body, turn it into a dto, and hand it to the client to send to the function
			var result = await _books.CreateBookAsync(dto);
			// once we get a result (only a good one, the middleware will catch all the bad ones), we return it
			return StatusCode(StatusCodes.Status201Created, new { message = result });
		}

		[HttpGet("genre/{genre}")]
		[ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)] // when things go right, 200
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // when things go wrong, 502
		public async Task<IActionResult> GetByGenre([FromRoute] string genre)
		{
			var result = await _books.GetBooksByGenreAsync(genre);
			return Ok(result);
		}

		[HttpGet("genre/{genre}/id/{id}")]
		[ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)] // provided id/genre does not exist
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
		public async Task<IActionResult> GetById([FromRoute] string genre, [FromRoute] string id)
		{
			var result = await _books.GetBookByIdAsync(genre, id);
			if (result == null)
			{
				return NotFound();
			}
			else
			{
				return Ok(result);
			}
		}

		[HttpPut("genre/{genre}/id/{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)] // user provides bad data
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // the gateway api has an error
		public async Task<IActionResult> Update([FromRoute] string genre, [FromRoute] string id, [FromBody] BookCreateRequestDto dto)
		{
			await _books.UpdateBookAsync(genre, id, dto);
			return Ok();
		}

		[HttpDelete("genre/{genre}/id/{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // the gateway api has an error
		public async Task<IActionResult> Delete([FromRoute] string genre, [FromRoute] string id)
		{
			await _books.DeleteBookAsync(genre, id);
			return Ok();
		}
	}
}
