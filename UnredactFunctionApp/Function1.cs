using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace UnredactFunctionApp
{
    public class Function1
    {
        private readonly string endpoint = Environment.GetEnvironmentVariable("FORM_RECOGNIZER_ENDPOINT");
        private readonly string apiKey = Environment.GetEnvironmentVariable("FORM_RECOGNIZER_API_KEY");
        private readonly BlobServiceClient blobServiceClient;

        public Function1(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        [FunctionName("Function1")]
        public async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "StorageConnection:blobServiceUri")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", myBlob);

            bool piiDetected = false;

            foreach (AnalyzedDocument document in operation.Value.Documents)
            {
                foreach (var entity in document.Entities)
                {
                    if (entity.Category == "PersonalIdentifiableInformation")
                    {
                        piiDetected = true;
                        break;
                    }
                }
            }

            if (piiDetected)
            {
                log.LogInformation($"PII detected in blob {name}. Flagging the blob.");
                await FlagBlob(name);
            }
        }

        private async Task FlagBlob(string blobName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient("samples-workitems");
            var blobClient = containerClient.GetBlobClient(blobName);

            var metadata = new Dictionary<string, string>
            {
                { "PII", "true" }
            };

            await blobClient.SetMetadataAsync(metadata);
        }
    }
}
