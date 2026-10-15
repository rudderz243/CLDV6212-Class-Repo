using LibraryGatewayAPI.DTOs;

namespace LibraryGatewayAPI.Clients
{
	// this class will communicate with all of our book HTTP trigger functions
	// it will use the DTOs that we created to send, and to recieve, the data from the Functions
	// it will also contain all the helper methods that we will call in our actual controllers
	public class BooksTableClient
	{
		// because our functions are HTTP triggers, we need to make use of an HTTP client to talk to them
		private readonly HttpClient _http;

		public BooksTableClient(HttpClient http)
		{
			_http = http;
		}

		public async Task<string> CreateBookAsync(BookCreateRequestDto bookToCreate)
		{
			// store the response from the Function, after sending the information about the book 
			var response = await _http.PostAsJsonAsync("api/books", bookToCreate);
			// tell the HTTP client to let us know if there are any errors, so that they can be sent
			// and handled by our GlobalExceptionMiddleware we created
			response.EnsureSuccessStatusCode();
			// if it was actually succesful, we return what the Function gave back
			return await response.Content.ReadAsStringAsync();
		}
						//	list of books
		public async Task<IEnumerable<BookResponseDto>> GetBooksByGenreAsync(string genre)
		{
														// this will ensure genre is a valid string
			var response = await _http.GetAsync($"api/books/{Uri.EscapeDataString(genre)}");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<IEnumerable<BookResponseDto>>();
		}

		public async Task<BookResponseDto> GetBookAsync(string genre, string id)
		{
			var response = await _http.GetAsync($"/api/books/{Uri.EscapeDataString(genre)}/{Uri.EscapeDataString(id)}");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<BookResponseDto>();
		}

		public async Task UpdateBookAsync(string genre, string id, BookCreateRequestDto updatedBook)
		{
			var response = await _http.PutAsJsonAsync($"/api/books/{Uri.EscapeDataString(genre)}/{Uri.EscapeDataString(id)}", updatedBook);
			response.EnsureSuccessStatusCode();
		}

		public async Task DeleteBookAsync(string genre, string id)
		{
			var response = await _http.DeleteAsync($"/api/books/{Uri.EscapeDataString(genre)}/{Uri.EscapeDataString(id)}");
			// we don't want the global error handler to trigger when we get a 404, so we make it an excpetion
			if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
			{
				response.EnsureSuccessStatusCode();	
			}
		}
	}
}
