using System;
using System.IO;
using System.Text;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace computerVision_imageAnalysis
{
    class Program
    {
        private static string cogSvcKey;
        private static string cogSvcEndpoint;
        private static ComputerVisionClient cvClient;

        static async Task Main()
        {
            try 
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                cogSvcKey = configuration["CognitiveServiceKey"];

                ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
                cvClient = new ComputerVisionClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };

                string imageFile = "images/building.jpg";

                await AnalyzeImage(imageFile);

                await GenerateThumbnail(imageFile);
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        static List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
        {
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Tags,
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Brands,
            VisualFeatureTypes.Objects,
            VisualFeatureTypes.Adult
        };

        static async Task GenerateThumbnail(string imageFile)
        {
            using(var imageData = File.OpenRead(imageFile))
            {
                // generate a thumbnail
                var thumbnailStream = await cvClient.GenerateThumbnailInStreamAsync(100, 100, imageData, true);
                string thumbnailFileName = "thumbnail.png";
                using(Stream thumbnailFile = File.Create(thumbnailFileName))
                {
                    thumbnailStream.CopyTo(thumbnailFile);
                }
                Console.WriteLine($"Thumbnail saved in {thumbnailFileName}");
            }
        }

        static async Task AnalyzeImage(string imageFile)
        {
            using(var imageData = File.OpenRead(imageFile))
            {
                var analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, features);

                // get image description
                foreach(var caption in analysis.Description.Captions)
                {
                    Console.WriteLine($"Description: {caption.Text} (confidence: {caption.Confidence.ToString("P")})");
                }

                // get image tags
                if(analysis.Tags.Count > 0)
                {
                    Console.WriteLine("Tags: ");
                    foreach(var tag in analysis.Tags)
                    {
                        Console.WriteLine($"  -{tag.Name} (confidence: {tag.Confidence.ToString("P")})");
                    }
                }

                // get image categories
                List<LandmarksModel> landmarks = new List<LandmarksModel> {};
                Console.WriteLine("Categories: ");
                foreach(var category in analysis.Categories)
                {
                    Console.WriteLine($" -{category.Name} (confidence: {category.Score.ToString("P")})");

                    if(category.Detail?.Landmarks != null)
                    {
                        foreach(LandmarksModel landmark in category.Detail.Landmarks)
                        {
                            if(!landmarks.Any(item => item.Name == landmark.Name))
                            {
                                landmarks.Add(landmark);
                            }
                        }
                    }
                }
                if(landmarks.Count > 0)
                {
                    Console.WriteLine("Landmarks:");
                    foreach(LandmarksModel landmark in landmarks)
                    {
                        Console.WriteLine($" -{landmark.Name} (confidence: {landmark.Confidence.ToString("P")})");
                    }
                }

                // get brands in the image
                if(analysis.Brands.Count > 0)
                {
                    Console.WriteLine("Brands:");
                    foreach(var brand in analysis.Brands)
                    {
                        Console.WriteLine($" -{brand.Name} (confidence: {brand.Confidence.ToString("P")})");
                    }
                }

                // get objects in the image
                if(analysis.Objects.Count > 0) 
                {
                    Console.WriteLine("Objects in image:");
                    Image image = Image.FromFile(imageFile);
                    Graphics graphics = Graphics.FromImage(image);
                    Pen pen = new Pen(Color.Cyan, 3);
                    Font font = new Font("Arial", 16);
                    SolidBrush brush = new SolidBrush(Color.Black);

                    foreach(var detectedObject in analysis.Objects)
                    {
                        Console.WriteLine($" -{detectedObject.ObjectProperty} (confidence: {detectedObject.Confidence.ToString("P")})");
                        var r = detectedObject.Rectangle;
                        Rectangle rect = new Rectangle(r.X, r.Y, r.W, r.H);
                        graphics.DrawRectangle(pen, rect);
                        graphics.DrawString(detectedObject.ObjectProperty, font, brush, r.X, r.Y);
                    }

                    String output_file = "objects.jpg";
                    image.Save(output_file);
                    Console.WriteLine("    Results saved in " + output_file);
                }

                // get moderation ratings
                string ratings = $"Ratings:\n -Adult: {analysis.Adult.IsAdultContent}\n -Racy: {analysis.Adult.IsRacyContent}\n -Gore: {analysis.Adult.IsGoryContent}";
                Console.WriteLine(ratings);
            }
        }
    }
}