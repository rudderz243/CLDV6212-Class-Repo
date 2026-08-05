using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// here you would call the connection string from your local.settings.json, but not today
const string connectionString = "UseDevelopmentStorage=true";

// we call in a new singleton for our Azurite services - remember - a singleton is a single instance of a service
builder.Services.AddSingleton(new TableServiceClient(connectionString));
builder.Services.AddSingleton(new BlobServiceClient(connectionString));

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
	builder.Services.AddOpenTelemetry()
		.UseFunctionsWorkerDefaults()
		.UseAzureMonitorExporter();
}

builder.Build().Run();
