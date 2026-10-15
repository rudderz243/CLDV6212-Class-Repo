using LibraryGatewayAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LibraryGatewayAPI.Clients
{
	// this class will communicate with all of our book HTTP trigger functions
	// it will use the DTOs we created to send and get back required data
	// it contains the helper methods so that we don't have to write the logic in the controller
	public class BooksTableClient
	{
		// because our functions are HTTP triggers, we need to use an HTTP client to communicate
		private readonly HttpClient _http;

		public BooksTableClient(HttpClient http)
		{
			_http = http;
		}

		// CreateBookAsync -> gets the required information to add a new book to the table using the Function
		public async Task<string> CreateBookAsync(BookCreateRequestDto bookToCreate)
		{
			// get the response back from the API after sending our request
			var response = await _http.PostAsJsonAsync("api/books", bookToCreate);
			// tell the http client to error out if the request is not successful
			// this will then trigger the GlobalExceptionMiddleware we created
			response.EnsureSuccessStatusCode();
			// if successful, return the response we got
			return await response.Content.ReadAsStringAsync();
		}

		public async Task<IEnumerable<BookResponseDto>> GetBooksByGenreAsync(string genre)
		{
			// Uri.EscapeDataString will ensure that the entered genre is clean and does not break the request
			var response = await _http.GetAsync($"api/books/{Uri.EscapeDataString(genre)}");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<IEnumerable<BookResponseDto>>();
		}
		// the ? in the type here is in case the specific book we want does not exist (it can return null)
		public async Task<BookResponseDto?> GetBookByIdAsync(string genre, string id)
		{
			var response = await _http.GetAsync($"api/books/{Uri.EscapeDataString(genre)}/{Uri.EscapeDataString(id)}");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<BookResponseDto>();
		}

		public async Task UpdateBookAsync(string genre, string id, BookCreateRequestDto updatedBookInformation)
		{
			var response = await _http.PutAsJsonAsync($"api/books/{Uri.EscapeDataString(genre)}/{Uri.EscapeDataString(id)}", updatedBookInformation);
			response.EnsureSuccessStatusCode();
		}

		public async Task DeleteBookAsync(string genre, string id)
		{
			var response = await _http.DeleteAsync($"api/books/{Uri.EscapeDataString(genre)}/{Uri.EscapeDataString(id)}");
			if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
			{
				response.EnsureSuccessStatusCode();
			}
		}
	}
}
