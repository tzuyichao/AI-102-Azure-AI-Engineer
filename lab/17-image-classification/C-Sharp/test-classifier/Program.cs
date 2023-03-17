using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace test_classifier
{
    class Program
    {
        static CustomVisionPredictionClient predictionClient;

        static void Main() 
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string predictionEndpoint = configuration["PredictionEndpoint"];
                string predictinoKey = configuration["PredictionKey"];
                Guid projectId = Guid.Parse(configuration["ProjectID"]);
                string modelName = configuration["ModelName"];

                predictionClient = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(predictinoKey))
                {
                    Endpoint = predictionEndpoint
                };

                String[] images = Directory.GetFiles("test-images");
                foreach(var image in images)
                {
                    Console.WriteLine(image + ": ");
                    MemoryStream imageData = new MemoryStream(File.ReadAllBytes(image));
                    var result = predictionClient.ClassifyImage(projectId, modelName, imageData);

                    foreach(var prediction in result.Predictions)
                    {
                        if(prediction.Probability > 0.5)
                        {
                            Console.WriteLine($"{prediction.TagName} ({prediction.Probability:P1})");
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