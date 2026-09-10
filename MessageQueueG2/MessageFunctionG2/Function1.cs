using System;
using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using MessageFunctionG2.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MessageFunctionG2;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    // TableClient -> how we interact with the table in Azurite
    private readonly TableClient _tableClient;
    private const string connectionString = "UseDevelopmentStorage=True;";
    private const string tableName = "ProcessedMessages";


    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
        // 1: prepare our connection to the table
        _tableClient = new TableClient(connectionString, tableName);
        // 2: initialize the connection
        _tableClient.CreateIfNotExists();

    }

    [Function(nameof(Function1))]
    public async Task Run([QueueTrigger("incoming-messages", Connection = "conn")] MessagePayload payload)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", payload.Id);

        var newTableEntry = new MessageEntity
        {
            PartitionKey = payload.Sender,
            RowKey = payload.Id.ToString(),
            Sender = payload.Sender,
            Content = payload.Content,
            SentAt = payload.SentAt
        };

        await _tableClient.AddEntityAsync(newTableEntry);
        _logger.LogInformation("Item added to the table");
    }
}