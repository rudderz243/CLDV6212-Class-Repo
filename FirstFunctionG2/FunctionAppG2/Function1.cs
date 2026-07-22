using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionAppG2;

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

        // get the name that is being passed by the MVC app
        string name = req.Query["name"];

        if (string.IsNullOrEmpty(name))
        {
            // write out response message into a new variable
            string response = "Please ensure that you pass a valid name through.";
            // and return that message back to the client / MVC application
            return new OkObjectResult(response);
        } else {
            // if a valid name WAS passed through, return an appropriate greeting
            string response = $"Howzit, {name}. Welcome to Azure Functions.";
			return new OkObjectResult(response);
		}
    }
}