using LibraryGatewayAPI.DTOs;

namespace LibraryGatewayAPI.Clients
{
	public class LoanQueueClient
	{
		private readonly HttpClient _http;

		public LoanQueueClient(HttpClient http)
		{
			_http = http;
		}

		public async Task<string> RequestLoanAsync(LoanSubmissionRequestDto loanInfo)
		{
			var response = await _http.PostAsJsonAsync("api/loans/queue", loanInfo);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}
	}
}
