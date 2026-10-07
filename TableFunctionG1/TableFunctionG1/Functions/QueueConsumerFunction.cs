using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TableFunctionG1.Models;

// queue trigger function -> queue name loan-queue
// the purpose of this function is to recieve the loans that are added into the queue, decode them, use DTO mapping to turn them into a Table Entity, then add them to the Table
namespace TableFunctionG1.Functions;

public class QueueConsumerFunction
{
    // the table client gives us access to the Azurite tables (so we can store the loans)
    private readonly TableClient _tableClient;
    private readonly ILogger<QueueConsumerFunction> _logger;

    public QueueConsumerFunction(ILogger<QueueConsumerFunction> logger, TableServiceClient singleton)
    {
        _logger = logger;
        _tableClient = singleton.GetTableClient("loans"); // name of table
        _tableClient.CreateIfNotExists();
    }

    [Function(nameof(QueueConsumerFunction))]
    public void Run([QueueTrigger("loan-queue", Connection = "")] QueueMessage message)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);

        // gets the item out of the queue, and turns it from JSON back into a LoanQueueMessage object
        var queueItem = JsonSerializer.Deserialize<LoanQueueMessage>(message.Body);

        // firstly, check if the JSON was valid
        if (queueItem is null)
        {
            _logger.LogWarning("unreadable queue message, skipping...");
            return;
        }

        // if it is a valid queue message, we convert it from a QueueMessage into a TableEntity
        var newEntity = new LoanEntity
        {
            BookId = queueItem.BookId,
            BorrowerId = queueItem.BorrowerId,
            LoanDate = queueItem.LoanDate,
            Status = "Active"
        };

        // once the entity is created and prepared, we can add it into the table
        _tableClient.AddEntity(newEntity);
        _logger.LogInformation("new item added to the table successfully.");
    }
}