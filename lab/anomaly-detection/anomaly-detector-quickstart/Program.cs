using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure;
using Azure.AI.AnomalyDetector;
using static System.Environment;

namespace anomaly_detector_quickstart
{
    internal class Program
    {
        static void Main()
        {
            string endpoint = GetEnvironmentVariable("ANOMALY_DETECTOR_ENDPOINT");
            string apiKey = GetEnvironmentVariable("ANOMALY_DETECTOR_API_KEY");

            var endpointUri = new Uri(endpoint);
            var credential = new AzureKeyCredential(apiKey);

            AnomalyDetectorClient clietn = new AnomalyDetectorClient(endpointUri, credential);

            string datapath = @"D:\lab\AI-102-Azure-AI-Engineer\lab\anomaly-detection\anomaly-detector-quickstart\request-data.csv";

            List<TimeSeriesPoint> list = File.ReadAllLines(datapath, Encoding.UTF8)
                .Where(e => e.Trim().Length != 0)
                .Select(e => e.Split(','))
                .Where(e => e.Length == 2)
                .Select(e => new TimeSeriesPoint(float.Parse(e[1])) 
                { 
                    Timestamp = DateTime.Parse(e[0])
                })
                .ToList();
            
            UnivariateDetectionOptions request = new UnivariateDetectionOptions(list)
            {
                Granularity = TimeGranularity.Daily
            };
            UnivariateEntireDetectionResult result = clietn.DetectUnivariateEntireSeries(request);
            bool hasAnomaly = false;
            for(int i=0; i<request.Series.Count; i++)
            {
                if(result.IsAnomaly[i])
                {
                    Console.WriteLine("Anomaly detected at index: {0}", i);
                    hasAnomaly = true;
                }
            }
            if(!hasAnomaly) 
            {
                Console.WriteLine("No anomalies detected in the series.");
            }
        }
    }
}