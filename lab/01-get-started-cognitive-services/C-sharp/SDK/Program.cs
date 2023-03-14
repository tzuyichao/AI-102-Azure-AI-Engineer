using System;
using Azure;
using Microsoft.Extensions.Configuration;
using System.Text;
using Azure.AI.TextAnalytics;

namespace sdk_client 
{
    class Program
    {
        private static string cogSvcEndpoint;
        private static string cogSvcKey;

        static void Main(string[] args) 
        {
            try 
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                
                cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                cogSvcKey = configuration["CognitiveServiceKey"];

                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;


                string userText = "";
                while(userText.ToLower() != "quit")
                {
                    Console.WriteLine("\nEnter some text('quit' to stop)");
                    userText = Console.ReadLine();
                    if(userText.ToLower() != "quit")
                    {
                        string language = GetLanguage(userText);
                        Console.WriteLine("Language: " + language);
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static string GetLanguage(string text)
        {
            AzureKeyCredential credential = new AzureKeyCredential(cogSvcKey);
            Uri endpoint = new Uri(cogSvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credential);

            DetectedLanguage detectedLanguage = client.DetectLanguage(text);
            return detectedLanguage.Name;
        }
    }
}