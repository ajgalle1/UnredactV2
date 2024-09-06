using Microsoft.AspNetCore.Components.Forms;
using UnredactV2.Services;

namespace UnredactV2.Logic
{
    public class DataLogic
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly string _containerName;

        public DataLogic(BlobStorageService blobStorageService, string containerName)
        {
            _blobStorageService = blobStorageService;
            _containerName = containerName;
        }

        public IBrowserFile SelectedFile { get; private set; }
        public bool Uploading { get; private set; }
        public string UploadResult { get; private set; }

        public void HandleFileSelected(InputFileChangeEventArgs e)
        {
            SelectedFile = e.File;
        }

        public async Task HandleValidSubmit()
        {
            if (SelectedFile != null)
            {
                Uploading = true;
                try
                {
                    var blobName = SelectedFile.Name;

                    using (var stream = SelectedFile.OpenReadStream())
                    {
                        await _blobStorageService.UploadFileAsync(_containerName, blobName, stream);
                    }

                    UploadResult = "File uploaded successfully!";
                }
                catch (Exception ex)
                {
                    UploadResult = $"Error uploading file: {ex.Message}";
                }
                finally
                {
                    Uploading = false;
                }
            }
            else
            {
                UploadResult = "Please select a file to upload.";
            }
        }







        private Random _random = new Random();
        // Your code here

        //TODO: User uploads a file which is saved in blob storage


        //TODO: Uploaded file is converted to text in azure AI service

        public bool DetermineIfPii()
        {

            if (_random.Next(0, 12) == 1)
            {
                return true;
            }
            else return false;
        }

        public bool DetermineIfSentimentNegative()
        {
            if (_random.Next(0, 12) == 1)
            {
                return true;
            }
            else return false;
        }

        public bool DetermineIfRedactionIneffective()
        {
            if (_random.Next(0, 12) == 1)
            {
                return true;
            }
            else return false;
        }
    }
}
