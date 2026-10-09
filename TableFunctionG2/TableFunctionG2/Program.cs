using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using System.Data.Common;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// call our connection string to talk to Azurite
string connectionString = builder.Configuration["AzureWebJobsStorage"];
// add our Azurite services using dependency injection
builder.Services.AddSingleton(new TableServiceClient(connectionString));
builder.Services.AddSingleton(new BlobServiceClient(connectionString));
builder.Services.AddSingleton(new QueueServiceClient(connectionString, new QueueClientOptions
{
	MessageEncoding = QueueMessageEncoding.Base64
}));

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
	builder.Services.AddOpenTelemetry()
		.UseFunctionsWorkerDefaults()
		.UseAzureMonitorExporter();
}


builder.Build().Run();
