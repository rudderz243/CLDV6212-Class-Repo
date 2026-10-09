using System;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TableFunctionG2.Models;

// timer trigger function
// the purpose of the timer function is to automatically update the loan status at a set interval
namespace TableFunctionG2;

public class TimerFunction
{
    // we need to access the loans table, in order to adjust the status
    private readonly TableClient _tableClient;
    private readonly ILogger _logger;

    public TimerFunction(ILoggerFactory loggerFactory, TableServiceClient singleton)
    {
        _logger = loggerFactory.CreateLogger<TimerFunction>();
        _tableClient = singleton.GetTableClient("loans");
        _tableClient.CreateIfNotExists();
    }

    [Function("TimerFunction")]
    public async Task RunAsync([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        // fetch all items from the loans table
        var loanItems = _tableClient.QueryAsync<LoanEntity>();
        // variable to keep track of how many items the timer updates
        int updatedItems = 0;

        // loop through each and every item, and update it respectively
        await foreach (LoanEntity loan in loanItems)
        {
            switch (loan.Status)
            {
                case "Active":
                    loan.Status = "Overdue";
                    updatedItems++;
                    await _tableClient.UpdateEntityAsync(loan, ETag.All, TableUpdateMode.Replace);
                    break;
				case "Overdue":
					loan.Status = "Returned";
					updatedItems++;
					await _tableClient.UpdateEntityAsync(loan, ETag.All, TableUpdateMode.Replace);
					break;
				case "Returned":
					loan.Status = "Archived";
					updatedItems++;
					await _tableClient.UpdateEntityAsync(loan, ETag.All, TableUpdateMode.Replace);
					break;
                case "Archived":
                    // do nothing -> archived is the final status a book loan can reach
                    break;
                default:
                    _logger.LogWarning("This loan has an unknown status -" + loan.RowKey);
                    break;
			}
        }
        _logger.LogInformation("The total number of items updated this time was: " + updatedItems);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}