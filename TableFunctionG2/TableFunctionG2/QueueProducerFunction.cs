using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TableFunctionG2.Models;

namespace TableFunctionG2;
// http trigger function
public class QueueProducerFunction
{
    // call in the queue client singleton from program.cs
    private readonly QueueClient _queueClient;
    
    private readonly ILogger<QueueProducerFunction> _logger;

    public QueueProducerFunction(ILogger<QueueProducerFunction> logger, QueueServiceClient singleton)
    {
        _logger = logger;
        // use the singleton
        _queueClient = singleton.GetQueueClient("loan-queue");
        _queueClient.CreateIfNotExists();
    }

    [Function("QueueProducerFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="loans/queue")] 
    HttpRequestData req)
    {
        _logger.LogInformation("Adding a new item to the queue.");

        // turn the data that was passed through from Postman in JSON format into an object of our class
        var newLoan = JsonSerializer.Deserialize<LoanQueueMessage>(req.Body);

        if (newLoan is null)
        {
            var nullResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await nullResponse.WriteStringAsync("invalid JSON sent, unable to parse data");
            return nullResponse;
        }

        if (newLoan.BookId is null || newLoan.BorrowerId is null)
        {
            var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("please ensure bookid and borrowerid are provided");
            return badResponse;
        }

        // if the newLoan is valid (so it passes all our error checks), we can then turn it into the correct
        // format to add to the queue
        string newLoanJson = JsonSerializer.Serialize(newLoan);
        await _queueClient.SendMessageAsync(newLoanJson);

        var goodResponse = req.CreateResponse(System.Net.HttpStatusCode.Created);
        await goodResponse.WriteAsJsonAsync(newLoan);
        return goodResponse;
    }
}