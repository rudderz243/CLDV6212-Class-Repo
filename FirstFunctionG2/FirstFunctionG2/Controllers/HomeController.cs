using FirstFunctionG2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FirstFunctionG2.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Index(string userName) {
			// check whether username is present or not
			if (string.IsNullOrEmpty(userName))
			{
				ViewBag.FuncResponse = "Please enter a valid name.";
				return View();
			}

			// set up the connection to our function
			var functionUrl = "http://localhost:7240/api/ProcessDataFunction";

			// in order to pass data through to a function, we need to either use the BODY or the PARAMETERS of the request
			// when specifying a parameter, you specify the name using a ? to indicate the beginning, and an = for the end
			// ?name= means that "name" is the parameter name, anything after the = is the value.
			var apiRequestUrl = $"{functionUrl}?name={userName}";

			try {
				// the next few lines in between the try { brackets } will attempt to run, and if anything bad happens
				// the catch { brackets } will run instead of the code completing

				using (var client = new HttpClient()) {
					// when we talk to a server/function/api, we do so by making a REQUEST
					// what we get back, is called a RESPONSE
					var response = await client.GetAsync(apiRequestUrl);
					
					// after we receieve a response, we need to first check whether the server was successful\
					// in running our code, or not
					if (response.IsSuccessStatusCode) {
						// if the function ran successfully, get the content it sent back to us, as a string
						var result = await response.Content.ReadAsStringAsync();
						// pass through the content to the frontend
						ViewBag.FuncResponse = result;
					} else {
						// if it was NOT successful, then:
						ViewBag.FuncResponse = $"Error Code: {response.StatusCode}";
					}
				}
			} catch (HttpRequestException ex) {
				// if an exception occurs attempting to make an HTTP request, print out the error message
				ViewBag.FuncResponse = $"Error: {ex.Message}";
			}
			// show the page now that we've fetched the content from the function
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
