using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FunctionAppG1;

public class ProcessDataFunction
{
    private readonly ILogger<ProcessDataFunction> _logger;

    public ProcessDataFunction(ILogger<ProcessDataFunction> logger)
    {
        _logger = logger;
    }

    [Function("ProcessDataFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        // get the parameters from the request
        string name = req.Query["name"];

        // if there is no name
        if (string.IsNullOrEmpty(name)) {
            // create an appropriate error message to return
            string error = "Please enter a valid name.";
            // return the message to the user / client
			return new OkObjectResult(error);
		} else {
            // we return a greeting response to the user
            string response = $"Howzit, {name}, welcome to Azure Functions.";
			// return the message to the user / client
			return new OkObjectResult(response);
		}
    }
}