using System;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.TextAnalytics;

namespace get_started
{
    class Program
    {
        static AzureKeyCredential credential;
        static Uri endpointUri;
        static async Task Main() 
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            var key = configuration["CognitiveServiceApiKey"];
            var endpoint = configuration["CognitiveServiceEndpoint"];
            endpointUri = new Uri(endpoint);
            credential = new AzureKeyCredential(key);

            var client = new TextAnalyticsClient(endpointUri, credential);
            await RecognizePIIExample(client);

            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        static async Task RecognizePIIExample(TextAnalyticsClient client)
        {
            string document = "Call our office at 312-555-1234, or send an email to support@contoso.com.";
            Response<CategorizedEntityCollection> response = await client.RecognizeEntitiesAsync(document).ConfigureAwait(false);
            foreach(var entity in response.Value)
            {
                Console.WriteLine($"Text: {entity.Text}, category: {entity.Category}, subcategory: {entity.SubCategory}, score: {entity.ConfidenceScore}");
            }
        }
    }
}