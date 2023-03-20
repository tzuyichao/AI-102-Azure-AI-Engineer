using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure;
using Azure.AI.AnomalyDetector;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace univariate_latest
{
    class Program
    {
        static void Main()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfiguration configuration = builder.Build();

            string endpoint = configuration["AnomalyDetectorEndpoint"];
            string apiKey = configuration["AnomalyDetectorApiKey"];

            var endpointUri = new Uri(endpoint);
            var credential = new AzureKeyCredential(apiKey);

            AnomalyDetectorClient client = new AnomalyDetectorClient(endpointUri, credential);

            string datapath = @"D:\lab\AI-102-Azure-AI-Engineer\lab\anomaly-detection\anomaly-detector-quickstart\request-data.csv";
            List<TimeSeriesPoint> list = File.ReadAllLines(datapath, Encoding.UTF8)
                .Where(e => e.Trim().Length != 0)
                .Select(e => e.Split(','))
                .Where(e => e.Length == 2)
                .Select(e => new TimeSeriesPoint(float.Parse(e[1])) { Timestamp = DateTime.Parse(e[0]) }).ToList();
            UnivariateDetectionOptions request = new UnivariateDetectionOptions(list)
            {
                Granularity = TimeGranularity.Daily
            };
            UnivariateLastDetectionResult result = client.DetectUnivariateLastPoint(request);
            Console.WriteLine($"isAnomaly: {result.IsAnomaly}, expectedValue: {result.ExpectedValue}");
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}