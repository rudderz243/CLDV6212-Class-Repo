using FunctionCRUDG1.Services;

namespace FunctionCRUDG1
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// call in and create the singleton for our API service

			var service = functionService.Instance;
		}
	}
}
