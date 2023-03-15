using System;
using System.IO;
using System.Text;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;

namespace text_analytics
{
    class Program
    {
        async static Task Main() 
        {
            try 
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;

                AzureKeyCredential credentials = new AzureKeyCredential(cogSvcKey);
                Uri endpoint = new Uri(cogSvcEndpoint);
                TextAnalyticsClient CogClient = new TextAnalyticsClient(endpoint, credentials);

                var folderPath = Path.GetFullPath("./reviews");
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                foreach(var file in folder.GetFiles("*.txt"))
                {
                    Console.WriteLine("\n----------------\n" + file.Name);
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    Console.WriteLine("\n" + text);

                    // Get language
                    Response<DetectedLanguage> detectedLanguage = await CogClient.DetectLanguageAsync(text);
                    Console.WriteLine($"\nLanguage: {detectedLanguage.Value.Name}");

                    // Get sentiment
                    Response<DocumentSentiment> sentimentAnalysis = await CogClient.AnalyzeSentimentAsync(text);
                    Console.WriteLine($"\nSentiment: {sentimentAnalysis.Value.Sentiment}");

                    // Get Key phrases
                    Response<KeyPhraseCollection> phrases = await CogClient.ExtractKeyPhrasesAsync(text);
                    if(phrases.Value.Count > 0)
                    {
                        Console.WriteLine("\nKey Phrases:");
                        foreach(string phrase in phrases.Value)
                        {
                            Console.WriteLine($"\t{phrase}");
                        }
                    }

                    // Get entities
                    Response<CategorizedEntityCollection> entities = await CogClient.RecognizeEntitiesAsync(text);
                    if(entities.Value.Count > 0)
                    {
                        Console.WriteLine("\nEntities:");
                        foreach(CategorizedEntity entity in entities.Value)
                        {
                            Console.WriteLine($"\t{entity.Text} ({entity.Category})");
                        }
                    }

                    // Get linked entities
                    Response<LinkedEntityCollection> linkedEntities = await CogClient.RecognizeLinkedEntitiesAsync(text);
                    if(linkedEntities.Value.Count > 0)
                    {
                        Console.WriteLine("\nLinks:");
                        foreach(LinkedEntity linkedEntity in linkedEntities.Value)
                        {
                            Console.WriteLine($"\t{linkedEntity.Name} ({linkedEntity.Url})");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}