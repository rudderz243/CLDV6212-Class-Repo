using System;
using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TableFunctionG1.Models;

// queue trigger function, points at (queue name)-poison
namespace TableFunctionG1.Functions;

public class PoisonQueueFunction
{
    private readonly TableClient _tableClient; // storing the logs in a table
    private readonly ILogger<PoisonQueueFunction> _logger;

    public PoisonQueueFunction(ILogger<PoisonQueueFunction> logger, TableServiceClient singleton)
    {
        _logger = logger;
        _tableClient = singleton.GetTableClient("failedLoans"); // failed(table name)
        _tableClient.CreateIfNotExists();
    }

    [Function(nameof(PoisonQueueFunction))]
    public async Task Run([QueueTrigger("loan-queue-poison", Connection = "")] QueueMessage message)
    {
        // we logCritical because this is very bad :(
        _logger.LogCritical("Poisoned message arrived: {messageText}", message.MessageText);

        // we then need to get the queue item into text so we can store it in the table
        string rawMessage = message.MessageText;

        // once we have the message, we prep our table entity
        var newEntity = new FailedLoanEntity
        {
            Payload = rawMessage,
            FailureReason = "Reached max dequeue attempts",
            DequeueCount = message.DequeueCount,
            EnqueuedTime = message.InsertedOn
        };

        // dump it in the table
        await _tableClient.AddEntityAsync(newEntity);
        _logger.LogInformation($"information regarding the poisoned item was stored in the table");
    }
}