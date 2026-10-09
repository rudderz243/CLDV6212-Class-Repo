using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// here you would call the connection string from your local.settings.json, but not today
//const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;";
const string connectionString = "UseDevelopmentStorage=true;";
// we call in a new singleton for our Azurite services - remember - a singleton is a single instance of a service
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
