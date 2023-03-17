using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace analyze_faces 
{
    class Program
    {
        static FaceClient faceClient;

        static async Task Main() 
        {
            try 
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfiguration configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
                faceClient = new FaceClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };

                Console.WriteLine("1: Detect faces\nAny other key to quit");
                Console.WriteLine("Enter a number:");
                string command = Console.ReadLine();
                switch(command)
                {
                    case "1":
                        await DetectFaces("images/people.jpg");
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task DetectFaces(string imageFile)
        {
            Console.WriteLine($"Detecting faces in {imageFile}");

            // Specify facial features to be retrieved
            List<FaceAttributeType?> features = new List<FaceAttributeType?>
            {
                FaceAttributeType.Occlusion,
                FaceAttributeType.Blur,
                FaceAttributeType.Glasses
            };

            // Get faces
            using (var imageData = File.OpenRead(imageFile))
            {    
                var detected_faces = await faceClient.Face.DetectWithStreamAsync(imageData, returnFaceAttributes: features, returnFaceId: false);

                if (detected_faces.Count > 0)
                {
                    Console.WriteLine($"{detected_faces.Count} faces detected.");

                    // Prepare image for drawing
                    Image image = Image.FromFile(imageFile);
                    Graphics graphics = Graphics.FromImage(image);
                    Pen pen = new Pen(Color.LightGreen, 3);
                    Font font = new Font("Arial", 4);
                    SolidBrush brush = new SolidBrush(Color.Black);
                    int faceCount=0;

                    // Draw and annotate each face
                    foreach (var face in detected_faces)
                    {
                        faceCount++;
                        Console.WriteLine($"\nFace number {faceCount}");
                        
                        // Get face properties
                        Console.WriteLine($" - Mouth Occluded: {face.FaceAttributes.Occlusion.MouthOccluded}");
                        Console.WriteLine($" - Eye Occluded: {face.FaceAttributes.Occlusion.EyeOccluded}");
                        Console.WriteLine($" - Blur: {face.FaceAttributes.Blur.BlurLevel}");
                        Console.WriteLine($" - Glasses: {face.FaceAttributes.Glasses}");

                        // Draw and annotate face
                        var r = face.FaceRectangle;
                        Rectangle rect = new Rectangle(r.Left, r.Top, r.Width, r.Height);
                        graphics.DrawRectangle(pen, rect);
                        string annotation = $"Face ID: {faceCount}";
                        graphics.DrawString(annotation,font,brush,r.Left, r.Top);
                    }

                    // Save annotated image
                    String output_file = "detected_faces.jpg";
                    image.Save(output_file);
                    Console.WriteLine(" Results saved in " + output_file);   
                }
            }
        }
    }
}