using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public Function1()
        {
            string blobServiceClientConnectionString = Environment.GetEnvironmentVariable("StorageConnection");
            if (string.IsNullOrEmpty(blobServiceClientConnectionString))
            {
                throw new ArgumentNullException("StorageConnection", "The StorageConnection environment variable is not set.");
            }
            this.blobServiceClient = new BlobServiceClient(blobServiceClientConnectionString);
        }

        [FunctionName("Function1")]
        public async Task Run([BlobTrigger("unredactsacontainer/{name}", Connection = "StorageConnection")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var containerClient = blobServiceClient.GetBlobContainerClient("unredactsacontainer");
            var blobClient = containerClient.GetBlobClient(name);

            // Check if the blob has already been processed
            var properties = await blobClient.GetPropertiesAsync();
            if (properties.Value.Metadata.ContainsKey("Processed") && properties.Value.Metadata["Processed"] == "true")
            {
                log.LogInformation($"Blob {name} has already been processed. Skipping.");
                return;
            }

            var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            AnalyzeDocumentOperation operation;
            try
            {
                log.LogInformation("Starting document analysis...");
                operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", myBlob);
                log.LogInformation("Document analysis completed.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error analyzing document: {ex.Message}");
                return;
            }

            bool piiDetected = false;
            StringBuilder extractedText = new StringBuilder();

            // Access the AnalyzeResult from the operation
            var analyzeResult = operation.Value;

            // Check if analyzeResult is null
            if (analyzeResult == null)
            {
                log.LogError("AnalyzeResult is null.");
                return;
            }

            // Iterate over the documents in the AnalyzeResult
            foreach (var document in analyzeResult.Documents)
            {
                log.LogInformation($"Document type: {document.DocumentType}");
                // Iterate over the key-value pairs in each document
                foreach (var kvp in document.Fields)
                {
                    var field = kvp.Value;
                    log.LogInformation($"Field: {kvp.Key}, Value: {field.Content}");
                    extractedText.AppendLine(field.Content);
                    if (field.FieldType == DocumentFieldType.String && field.Content.Contains("PII"))
                    {
                        piiDetected = true;
                    }
                }
            }

            // Log the extracted text
            log.LogInformation($"Extracted text from blob {name}:\n{extractedText}");

            if (piiDetected)
            {
                log.LogInformation($"PII detected in blob {name}. Flagging the blob.");
                await FlagBlob(name);
            }

            // Add the extracted text as metadata
            await AddMetadataToBlob(name, extractedText.ToString(), log);

            // Mark the blob as processed
            var metadata = new Dictionary<string, string>
            {
                { "Processed", "true" }
            };
            await blobClient.SetMetadataAsync(metadata);
            log.LogInformation($"Blob {name} marked as processed.");
        }

        private async Task FlagBlob(string blobName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient("unredactsacontainer");
            var blobClient = containerClient.GetBlobClient(blobName);

            var metadata = new Dictionary<string, string>
            {
                { "PII", "true" }
            };

            await blobClient.SetMetadataAsync(metadata);
        }

        private async Task AddMetadataToBlob(string blobName, string text, ILogger log)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient("unredactsacontainer");
            var blobClient = containerClient.GetBlobClient(blobName);

            // Truncate the text if it exceeds the metadata size limit
            string truncatedText = text.Length > 8000 ? text.Substring(0, 8000) : text;

            var metadata = new Dictionary<string, string>
            {
                { "ExtractedText", truncatedText }
            };

            try
            {
                await blobClient.SetMetadataAsync(metadata);
                log.LogInformation($"Metadata added to blob {blobName}.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error adding metadata to blob {blobName}: {ex.Message}");
            }
        }
    }
}
