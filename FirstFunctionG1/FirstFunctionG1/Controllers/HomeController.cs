using FirstFunctionG1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FirstFunctionG1.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Index (string userName) {
			// check if userName has been passed
			if (string.IsNullOrEmpty(userName))
			{
				// set an appropriate error message
				ViewBag.FuncResponse = "Please enter a valid name.";
				// return the View (page)
				return View();
			}

			// we need to specify the function URL of the function we want to interact with
			// the part after */api/ will be the name of the Function in your Functions app.
			var functionUrl = "http://localhost:7058/api/ProcessDataFunction";

			// in order to pass the userName attribute to the function, we make use of a parameter in the URL
			// the name of the parameter is everything between the ? and = (or & =), so ?name= is "name"
			// everything after the = is the value that is passed through to the api or function
			var apiRequest = $"{functionUrl}?name={userName}";


			try {
				// for the next few lines of code { between these brackets } , make use of an HttpClient
				// HttpCLient means a web client, we use it to make web requests to other server, apis, or functions
				using (var client = new HttpClient()) {
					// talk to the function, and get back its response
					var response = await client.GetAsync(apiRequest);
					// if the response executed successfully, and did not return an error status code
					if (response.IsSuccessStatusCode) {
						// get the result content, as a string 
						var result = await response.Content.ReadAsStringAsync();
						// pass this result to the frontend / view
						ViewBag.FuncResponse = result;
					} else {
						// otherwise, return the response error code so we can diagnose
						ViewBag.FuncResponse = $"Error Code: {response.StatusCode}";
					}
				}
			} catch (HttpRequestException e) {
				// pass the error to the frontend so we can read it
				ViewBag.FuncResponse = $"Error: {e.Message}";
			}

		
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
