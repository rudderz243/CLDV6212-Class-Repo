using LibraryGatewayAPI.Clients;
using LibraryGatewayAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryGatewayAPI.Controllers
{
	[Route("api/v1/books")]
	[ApiController]
	// every and all responses from this controller will be in JSON format
	[Produces("application/json")]
	
	public class BookController : ControllerBase
	{
		// call in our book client (which handles communication with the HTTP trigger functions for books)
		private readonly BooksTableClient _b;

		public BookController(BooksTableClient b)
		{
			_b = b;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)] // when things go well -> 201
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)] // user gives bad data -> 400
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // api had an error (not the function)
		public async Task<IActionResult> Create([FromBody] BookCreateRequestDto dto) // automatically converts from JSON
		{
			var result = _b.CreateBookAsync(dto);
			return Created();
		}

		[HttpGet("genre/{genre}")]
		[ProducesResponseType(typeof(IEnumerable<BookResponseDto>), StatusCodes.Status200OK)] // note -> returns multiple: ienumerable
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // api had an error (not the function)
		public async Task<IActionResult> GetByGenre([FromRoute] string genre)
		{
			var result = _b.GetBooksByGenreAsync(genre);
			return Ok(result); // return the books inside the 200 OK
		}

		[HttpGet("genre/{genre}/id/{id}")]
		[ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)] // note -> returning one? standard dto
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)] // api had an error (not the function)
		public async Task<IActionResult> GetById([FromRoute] string genre, [FromRoute] string id)
		{
			var result = _b.GetBookAsync(genre, id);

			if (result is null)
			{
				return NotFound(); 
			} else
			{
				return Ok(result);
			}
		}

		[HttpPut("genre/{genre}/id/{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)] // bad update data
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
		public async Task<IActionResult> Update([FromRoute] string genre, [FromRoute] string id, [FromBody] BookCreateRequestDto dto)
		{
			await _b.UpdateBookAsync(genre, id, dto);
			return Ok();
		}

		[HttpDelete("genre/{genre}/id/{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
		public async Task<IActionResult> Delete([FromRoute] string genre, [FromRoute] string id)
		{
			await _b.DeleteBookAsync(genre, id);
			return Ok();
		}
	}
}
