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

		public async Task<IActionResult> Index(string userName) {
			// check whether username is present or not
			if (string.IsNullOrEmpty(userName))
			{
				ViewBag.FuncResponse = "Please enter a valid name.";
				return View();
			}

			// set up the connection to our function
			var functionUrl = "http://localhost:7071/api/ProcessDataFunction";

			// in order to pass data through to a function, we need to either use the BODY or the PARAMETERS of the request
			// when specifying a parameter, you specify the name using a ? to indicate the beginning, and an = for the end
			// ?name= means that "name" is the parameter name, anything after the = is the value.
			var apiRequestUrl = $"{functionUrl}?name={userName}";
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
