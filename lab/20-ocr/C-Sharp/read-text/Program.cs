using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace read_text
{
    class Program
    {
        static ComputerVisionClient cvClient;

        static async Task Main()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfiguration configuration = builder.Build();
            string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
            string cogSvcKey = configuration["CognitiveServiceKey"];

            ApiKeyServiceClientCredentials credential = new ApiKeyServiceClientCredentials(cogSvcKey);
            cvClient = new ComputerVisionClient(credential)
            {
                Endpoint = cogSvcEndpoint
            };

            Console.WriteLine("1. Use Read API for image\n2. Use Read API for document\n3. Read handwriting\nAny other key to quit");
            Console.WriteLine("Enter a number:");
            string command = Console.ReadLine();
            string imageFile = "";
            switch (command)
            {
                case "1":
                    imageFile = "images/Lincoln.jpg";
                    break;
                case "2":
                    imageFile = "images/Rome.pdf";
                    break;
                case "3":
                    imageFile = "images/Note.jpg";
                    break;
                default:
                    break;
            }
            if(imageFile != "")
            {
                await GetTextRead(imageFile);
            }
        }

        static async Task GetTextRead(string imageFile)
        {
            Console.WriteLine($"Reading text in {imageFile}\n");
            using(var imageData = File.OpenRead(imageFile))
            {
                var readOp = await cvClient.ReadInStreamAsync(imageData);
                // Get the async operation ID so we can check for the results
                string operationLocation = readOp.OperationLocation;
                string operationId = operationLocation.Substring(operationLocation.Length - 36);

                // Wait for the asynchronous operation to complete
                ReadOperationResult results;
                do
                {
                    Thread.Sleep(1000);
                    results = await cvClient.GetReadResultAsync(Guid.Parse(operationId));
                }
                while ((results.Status == OperationStatusCodes.Running ||
                        results.Status == OperationStatusCodes.NotStarted));

                // If the operation was successfully, process the text line by line
                if (results.Status == OperationStatusCodes.Succeeded)
                {
                    var textUrlFileResults = results.AnalyzeResult.ReadResults;
                    foreach (ReadResult page in textUrlFileResults)
                    {
                        foreach (Line line in page.Lines)
                        {
                            Console.WriteLine(line.Text);
                            
                            Console.WriteLine(line.BoundingBox);
                        }
                    }
                }
            }
        }
    }
}