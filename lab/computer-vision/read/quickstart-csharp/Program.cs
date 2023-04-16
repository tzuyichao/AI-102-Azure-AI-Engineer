using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ComputerVisionGetstart
{
    class Program
    {
        static string key;
        static string endpoint;
        private const string READ_TEXT_URL_IMAGE = "https://raw.githubusercontent.com/Azure-Samples/cognitive-services-sample-data-files/master/ComputerVision/Images/printed_text.jpg";

        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            key = configuration["KEY"];
            endpoint = configuration["ENDPOINT"];

            Console.WriteLine("Azure Cognitive Services Computer Vision - .NET quickstart example");
            Console.WriteLine();

            ComputerVisionClient client = Authenticate(endpoint, key);

            ReadFileUrl(client, READ_TEXT_URL_IMAGE).Wait();
        }

        static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint=endpoint
            };
            return client;
        }

        static async Task ReadFileUrl(ComputerVisionClient client, string urlFile)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("READ FILE FROM URL");
            Console.WriteLine();

            // Read text from URL with a specific model version
            // var textHeaders = await client.ReadAsync(urlFile,null,null,"2022-04-30");

            var textHeader = await client.ReadAsync(urlFile);
            string operationLocation = textHeader.OperationLocation;
            Console.WriteLine($"Operation Location: {operationLocation}");
            Thread.Sleep(2000);

            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult results;
            Console.WriteLine($"Extracting text from URL file {Path.GetFileName(urlFile)}...");
            Console.WriteLine();
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while(results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

            Console.WriteLine();
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            foreach(ReadResult page in textUrlFileResults)
            {
                foreach(Line line in page.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();
        }
    }
}
