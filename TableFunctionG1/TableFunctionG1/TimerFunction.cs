using System;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TableFunctionG1.Models;

// timer trigger function
namespace TableFunctionG1;

public class TimerFunction
{
	private readonly TableClient _tableClient; // we need to access the table to adjust the loans
	private readonly ILogger _logger;

	public TimerFunction(ILoggerFactory loggerFactory, TableServiceClient singleton)
	{
		_logger = loggerFactory.CreateLogger<TimerFunction>();
		_tableClient = singleton.GetTableClient("loans"); // name of table we want
		_tableClient.CreateIfNotExists();
	}

	[Function("TimerFunction")]
	public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
	{
		_logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

		// fetch all of the items from the table
		var tableItems = _tableClient.QueryAsync<LoanEntity>();
		// create a variable to store how many items we've changed
		int updatedItems = 0;

		// we then loop through each and every item that was returned, and update it respectively
		await foreach (LoanEntity loan in tableItems)
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
					// do nothing, this is the final state
					break;
				default:
					// if the loan status is not one of the above somehow, log it and move on
					_logger.LogWarning($"Unknown loan status {loan.Status} for loan {loan.RowKey}");
					break;
			}
		}
		_logger.LogInformation($"Number of records updated this pass: {updatedItems}");



		if (myTimer.ScheduleStatus is not null)
		{
			_logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
		}
	}
}