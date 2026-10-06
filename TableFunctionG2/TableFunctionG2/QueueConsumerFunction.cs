using System;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TableFunctionG2.Models;

namespace TableFunctionG2;
// queue trigger function
public class QueueConsumerFunction
{
    private readonly TableClient _tableClient;
    private readonly ILogger<QueueConsumerFunction> _logger;

    public QueueConsumerFunction(ILogger<QueueConsumerFunction> logger, TableServiceClient singleton)
    {
        _logger = logger;
        _tableClient = singleton.GetTableClient("loans");
        _tableClient.CreateIfNotExists();
    }

    [Function(nameof(QueueConsumerFunction))]
    public void Run([QueueTrigger("loan-queue", Connection = "conn")] QueueMessage message)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);

        // reads the JSON information from the queue and turns it into an object we can work with
        var loanData = JsonSerializer.Deserialize<LoanQueueMessage>(message.Body);

        if (loanData is null)
        {
            _logger.LogWarning("Weird unreadable queue messaged appeared. skipping...");
            return;
        }

        // if the message is fine (which it should be) -> prepare the message to be added to the table
        var loanTableEntity = new LoanEntity
        {
            BookId = loanData.BookId,
            BorrowerId = loanData.BorrowerId,
            LoanDate = loanData.LoanDate,
            Status = "Active"
        };

        // once the entity is ready to be added to the table -> add it
        _tableClient.AddEntity(loanTableEntity);
        _logger.LogInformation("new table entity added");
    }
}