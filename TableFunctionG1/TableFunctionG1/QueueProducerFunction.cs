using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TableFunctionG1.Models;

// http trigger function -> authorization level anonymous
namespace TableFunctionG1;

public class QueueProducerFunction
{
    private readonly QueueClient _queueClient;
    private readonly ILogger<QueueProducerFunction> _logger;

    public QueueProducerFunction(ILogger<QueueProducerFunction> logger, QueueServiceClient singleton)
    {
        _logger = logger;
        _queueClient = singleton.GetQueueClient("loan-queue");
        _queueClient.CreateIfNotExists();
    }

    [Function("QueueProducerFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="loans/queue")]
    HttpRequestData req)
    {
        _logger.LogInformation("Adding a new item to the queue...");

        // first, we decode the JSON information from the request body, and turn it into an object
        var newLoan = JsonSerializer.Deserialize<LoanQueueMessage>(req.Body);

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