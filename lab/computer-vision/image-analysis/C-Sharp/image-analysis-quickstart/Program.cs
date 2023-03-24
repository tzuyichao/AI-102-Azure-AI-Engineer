using System;
using Microsoft.Extensions.Configuration;
using Azure.AI.Vision.Core.Input;
using Azure.AI.Vision.Core.Options;
using Azure.AI.Vision.ImageAnalysis;

namespace get_started
{
    class Program
    {
        static async Task AnalyzeImage()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            Uri endpoint = new Uri(configuration["VisionEndpoint"]);
            string apiKey = configuration["VisionApiKey"];
            VisionServiceOptions serviceOptions = new VisionServiceOptions(endpoint, apiKey);

            var imageSource = VisionSource.FromUrl(new Uri("https://learn.microsoft.com/azure/cognitive-services/computer-vision/media/quickstarts/presentation.png"));
            var analysisOptions = new ImageAnalysisOptions()
            {
                // ImageAnalysisFeature.Caption not support East Asia 2023/3/24
                Features = ImageAnalysisFeature.Tags | ImageAnalysisFeature.Text | ImageAnalysisFeature.People | ImageAnalysisFeature.Objects | ImageAnalysisFeature.CropSuggestions,
                Language = "en",
                GenderNeutralCaption = true
            };

            using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);
            var result = await analyzer.AnalyzeAsync();

            if(result.Reason == ImageAnalysisResultReason.Analyzed)
            {
                if(result.Caption != null)
                {
                    Console.WriteLine(" Caption:");
                    Console.WriteLine($"    \"{result.Caption.Content}\", Confidence {result.Caption.Confidence:0.0000}");
                }

                if (result.Text != null)
                {
                    Console.WriteLine($" Text:");
                    foreach (var line in result.Text.Lines)
                    {
                        string pointsToString = "{" + string.Join(',', line.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
                        Console.WriteLine($"   Line: '{line.Content}', Bounding polygon {pointsToString}");

                        foreach (var word in line.Words)
                        {
                            pointsToString = "{" + string.Join(',', word.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
                            Console.WriteLine($"     Word: '{word.Content}', Bounding polygon {pointsToString}, Confidence {word.Confidence:0.0000}");
                        }
                    }
                }

                if(result.Tags != null)
                {
                    Console.WriteLine($" Tags:");
                    foreach(var tag in result.Tags)
                    {
                        Console.WriteLine($"  Tag Name: {tag.Name}, Confidence: {tag.Confidence}");
                    }
                }

                if(result.People != null)
                {
                    Console.WriteLine(" People:");
                    foreach(var people in result.People)
                    {
                        
                        Console.WriteLine($"  {people.BoundingBox}, Confidence: {people.Confidence}");
                    }
                }

                if(result.Objects != null)
                {
                    Console.WriteLine(" Objects:");
                    foreach(var obj in result.Objects)
                    {
                        Console.WriteLine($"  Name: {obj.Name}, Position: {obj.BoundingBox}, Confidence: {obj.Confidence}");
                    }
                }

                if(result.CropSuggestions != null)
                {
                    Console.WriteLine(" CropSuggestions:");
                    foreach(var sugg in result.CropSuggestions)
                    {
                        Console.WriteLine($"  {sugg.BoundingBox} {sugg.AspectRatio}");
                    }
                }
            }
            else if (result.Reason == ImageAnalysisResultReason.Error)
            {
                var errorDetails = ImageAnalysisErrorDetails.FromResult(result);
                Console.WriteLine(" Analysis failed.");
                Console.WriteLine($"   Error reason : {errorDetails.Reason}");
                Console.WriteLine($"   Error code : {errorDetails.ErrorCode}");
                Console.WriteLine($"   Error message: {errorDetails.Message}");
            }
        }

        static async Task Main()
        {
            await AnalyzeImage().ConfigureAwait(false);
            Console.WriteLine("Done.");
        }
    }
}