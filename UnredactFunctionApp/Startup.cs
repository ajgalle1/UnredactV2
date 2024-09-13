using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(UnredactFunctionApp.Startup))]

namespace UnredactFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string blobServiceClientConnectionString = Environment.GetEnvironmentVariable("StorageConnection");
            builder.Services.AddSingleton(new BlobServiceClient(blobServiceClientConnectionString));
        }
    }
}