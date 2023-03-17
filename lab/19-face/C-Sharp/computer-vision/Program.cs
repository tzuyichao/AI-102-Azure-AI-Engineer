using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

// dotnet add package System.Drawing.Common
namespace detect_faces
{
    class Program
    {
        private static ComputerVisionClient cvClient;
        static async Task Main()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                // create client
                ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
                cvClient = new ComputerVisionClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };

                // detect faces in an image
                string imageFile = "images/people.jpg";
                await AnalyzeFaces(imageFile);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task AnalyzeFaces(string imageFile)
        {
            Console.WriteLine($"Analyzing {imageFile}");
            // specify features to be retrieved
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Faces
            };

            // get image analysis
            using(var imageData = File.OpenRead(imageFile))
            {
                var analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, features);

                Image image = Image.FromFile(imageFile);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.LightGreen, 3);
                Font font = new Font("Arial", 3);
                SolidBrush brush = new SolidBrush(Color.LightGreen);
                foreach(var face in analysis.Faces)
                {
                    var r = face.FaceRectangle;
                    Rectangle rect = new Rectangle(r.Left, r.Top, r.Width, r.Height);
                    graphics.DrawRectangle(pen, rect);
                    string annotation = $"Person at approximately {r.Left}, {r.Top}";
                    graphics.DrawString(annotation, font, brush, r.Left, r.Top);
                }

                String outputFile = "detected_faces.jpg";
                image.Save(outputFile);
                Console.WriteLine(" Result saved in " + outputFile);
            }
        }
    }
}