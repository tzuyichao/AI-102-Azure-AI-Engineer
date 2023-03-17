using System;
using System.IO;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace test_detector
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
                string trainingEndpoint = configuration["PredictionEndpoint"];
                string trainingkey = configuration["PredictionKey"];
                Guid projectId = Guid.Parse(configuration["ProjectID"]);
                string modelName = configuration["ModelName"];

                predictionClient = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(trainingkey))
                {
                    Endpoint = trainingEndpoint
                };

                // load image and prepare for drawing
                String imageFile = "produce.jpg";
                Image image = Image.FromFile(imageFile);
                int h = image.Height;
                int w = image.Width;
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Magenta, 3);
                Font font = new Font("Arial", 16);
                SolidBrush brush = new SolidBrush(Color.Black);

                using(var imageData = File.OpenRead(imageFile))
                {
                    Console.WriteLine("Detecting objects in " + imageFile);
                    var result = predictionClient.DetectImage(projectId, modelName, imageData);
                    foreach(var prediction in result.Predictions)
                    {
                        if(prediction.Probability > 0.5)
                        {
                            int left = Convert.ToInt32(prediction.BoundingBox.Left * w);
                            int top = Convert.ToInt32(prediction.BoundingBox.Top * h);
                            int height = Convert.ToInt32(prediction.BoundingBox.Height * w);
                            int width = Convert.ToInt32(prediction.BoundingBox.Width * h);

                            Rectangle rect = new Rectangle(left, top, width, height);
                            graphics.DrawRectangle(pen, rect);
                            graphics.DrawString(prediction.TagName, font, brush, left, top);
                        }
                    }
                }
                string outputFile = "output.jpg";
                image.Save(outputFile);
                Console.WriteLine("Results saved in " + outputFile);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}