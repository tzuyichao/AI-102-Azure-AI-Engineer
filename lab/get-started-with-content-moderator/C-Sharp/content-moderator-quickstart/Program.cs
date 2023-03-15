using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

// only Text and Image
// Human review 看微軟github的source
// https://github.com/Azure-Samples/cognitive-services-quickstart-code/blob/master/dotnet/ContentModerator/Program.cs
namespace contentModerator_quickstart 
{
    class Program 
    {
        private static readonly string SubcriptionKey = "YOUR_CONTENT_MODERATOR_SUBSCRIPTION_KEY";
        private static readonly string Endpoint = "YOUR_CONTENT_MODERATOR_ENDPOINT";

        // TEXT MODERATION
        // Name of the file that contains text
        private static readonly string TextFile = "TextFile.txt";
        // The name of the file to contain the output from the evaluation.
        private static string TextOutputFile = "TextModerationOutput.txt";

        // IMAGE MODERATION
        //The name of the file that contains the image URLs to evaluate.
        private static readonly string ImageUrlFile = "ImageFiles.txt";
        // The name of the file to contain the output from the evaluation.
        private static string ImageOutputFile = "ImageModerationOutput.json";

        static void Main()
        {
            ContentModeratorClient clientImage = Authenticate(SubcriptionKey, Endpoint);
            ContentModeratorClient clientText = Authenticate(SubcriptionKey, Endpoint);

            ModerateText(clientText, TextFile, TextOutputFile);
            ModerateImages(clientImage, ImageUrlFile, ImageOutputFile);

            Console.WriteLine("Done.");
        }

        public static ContentModeratorClient Authenticate(string key, string endpoint)
        {
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(key));
            client.Endpoint = endpoint;
            return client;
        }

        public static void ModerateText(ContentModeratorClient client, string inputFile, string outputFile)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("TEXT MODERATION");
            Console.WriteLine();
            
            string text = File.ReadAllText(inputFile);
            text = text.Replace(Environment.NewLine, " ");
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(textBytes);
            Console.WriteLine("Screening {0}...", inputFile);

            using(StreamWriter outputWriter = new StreamWriter(outputFile, false))
            {
                using(client)
                {
                    outputWriter.WriteLine("Autocorrect typos, check for matching terms, PII, and classify.");
                    var screenResult = client.TextModeration.ScreenText("text/plain", stream, "eng", true, true, null, true);
                    outputWriter.WriteLine(JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                }
                outputWriter.Flush();
                outputWriter.Close();
            }

            Console.WriteLine("Results written to {0}", outputFile);
            Console.WriteLine();
        }

        public class EvaluationData
        {
            public string ImageUrl;
            public Evaluate ImageModeration;
            public OCR TextDetection;
            public FoundFaces FaceDetection;
        }

        public static void ModerateImages(ContentModeratorClient client, string urlFile, string outputFile)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("IMAGE MODERATION");
            Console.WriteLine();

            List<EvaluationData> evaluationData = new List<EvaluationData>();

            using(client) 
            {
                using(StreamReader inputReader = new StreamReader(urlFile))
                {
                    while(!inputReader.EndOfStream)
                    {
                        string line = inputReader.ReadLine().Trim();
                        if(line != String.Empty)
                        {
                            Console.WriteLine("Evaluating {0}...", Path.GetFileName(line));
                            var imageUrl = new BodyModel("URL", line.Trim());
                            var imageData = new EvaluationData
                            {
                                ImageUrl = imageUrl.Value,
                                ImageModeration = client.ImageModeration.EvaluateUrlInput("application/json", imageUrl, true)
                            };
                            Thread.Sleep(1000);

                            // Detect and extract text
                            imageData.TextDetection = client.ImageModeration.OCRUrlInput("eng", "application/json", imageUrl, true);
                            Thread.Sleep(1000);

                            // Detect faces
                            imageData.FaceDetection = client.ImageModeration.FindFacesUrlInput("application/json", imageUrl, true);
                            Thread.Sleep(1000);

                            // add results to Evaluation object
                            evaluationData.Add(imageData);
                        }

                        using(StreamWriter outputWriter = new StreamWriter(outputFile, false))
                        {
                            outputWriter.WriteLine(JsonConvert.SerializeObject(evaluationData, Formatting.Indented));
                            outputWriter.Flush();
                            outputWriter.Close();
                        }
                        Console.WriteLine();
                        Console.WriteLine("Image moderation results written to output file: " + outputFile);
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}