using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TableFunctionG2.Models;

// queue trigger function
// points to (queue name)-poison. the poison queue is a special queue that gets created automatically, and is
// where any queue items that fail to get processed get sent
namespace TableFunctionG2;

public class PoisonHandlingFunction
{
    // we need the table client to store the failed queue items somewhere
    private readonly TableClient _tableClient;
    private readonly ILogger<PoisonHandlingFunction> _logger;

    public PoisonHandlingFunction(ILogger<PoisonHandlingFunction> logger, TableServiceClient singleton)
    {
        _logger = logger;
        _tableClient = singleton.GetTableClient("failedLoans");
        _tableClient.CreateIfNotExists();
    }

    [Function(nameof(PoisonHandlingFunction))]
    public async Task RunAsync([QueueTrigger("loan-queue-poison", Connection = "")] QueueMessage message)
    {
        // we change log to LogCritical (which will make angry red text in the terminal), as poison is bad
        _logger.LogCritical("Poisoned message arrived: {messageText}", message.MessageText);

        // we need to grab the message text so we can log it
        string messageText = message.MessageText;

        // once we have the text, we create the Entity object to store in the table
        var failedLoan = new FailedLoanEntity
        {
            Payload = messageText,
            FailureReason = "Reached max dequeue amount and became poisoned",
            DequeueCount = message.DequeueCount,
            EnqueueTime = message.InsertedOn
        };

        // add to table, and log
        await _tableClient.AddEntityAsync(failedLoan);
        _logger.LogInformation("item stored in table successfully.");   
    }
}