using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TableFunctionG1.Models;

// http trigger function -> authorization level anonymous
// the purpose of this function is to get in the information about each loan that is being made, and convert it into a format that can be added into the queue
namespace TableFunctionG1.Functions;

public class QueueProducerFunction
{
    // the queue client gives us access to the Azurite queue (so we can send the loans into the queue)
    private readonly QueueClient _queueClient;
    private readonly ILogger<QueueProducerFunction> _logger;

    public QueueProducerFunction(ILogger<QueueProducerFunction> logger, QueueServiceClient singleton)
    {
        _logger = logger;
        _queueClient = singleton.GetQueueClient("loan-queue"); // name of queue
        _queueClient.CreateIfNotExists();
    }

    [Function("QueueProducerFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="loans/queue")]
    HttpRequestData req)
    {
        _logger.LogInformation("Adding a new item to the queue...");

        // first, we decode the JSON information from the request body, and turn it into an object
        var newLoan = await JsonSerializer.DeserializeAsync<LoanQueueMessage>(req.Body);

        // check if we were able to decode the json
        if (newLoan is null)
        {
            var nullResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await nullResponse.WriteStringAsync("unable to decode from json into object");
            return nullResponse;
        }

        // check that all required information is present
        if (newLoan.BookId is null || newLoan.BorrowerId is null)
        {
            var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("not all required information is present");
            return badResponse;
        }

        // if the data passes all of our error checking, we prepare it to be added into the queue
        string queueItem = JsonSerializer.Serialize(newLoan);
        await _queueClient.SendMessageAsync(queueItem);

        // let the user know it was added succesfully
        var goodResponse = req.CreateResponse(System.Net.HttpStatusCode.Created);
        await goodResponse.WriteAsJsonAsync(newLoan);
        return goodResponse;
    }
}