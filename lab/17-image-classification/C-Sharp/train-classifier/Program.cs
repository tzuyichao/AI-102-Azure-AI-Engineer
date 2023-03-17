using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

namespace train_classifier
{
    class Program
    {
        static CustomVisionTrainingClient trainingClient;
        static Project customVisionProject;

        static void Main() 
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            string trainingEndpoint = configuration["TrainingEndpoint"];
            string trainingKey = configuration["TrainingKey"];
            Guid projectId = Guid.Parse(configuration["ProjectID"]);

            try 
            {
                var credential = new ApiKeyServiceClientCredentials(trainingKey);
                trainingClient = new CustomVisionTrainingClient(credential)
                {
                    Endpoint = trainingEndpoint
                };

                customVisionProject = trainingClient.GetProject(projectId);

                UploadImages("more-training-images");

                TrainModel();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void UploadImages(string folder)
        {
            Console.WriteLine("Upload images ...");
            IList<Tag> tags = trainingClient.GetTags(customVisionProject.Id);
            foreach(var tag in tags)
            {
                Console.Write(tag.Name);
                String[] images = Directory.GetFiles(Path.Combine(folder, tag.Name));
                foreach(var image in images)
                {
                    Console.Write(".");
                    using(var stream = new MemoryStream(File.ReadAllBytes(image)))
                    {
                        trainingClient.CreateImagesFromData(customVisionProject.Id, stream, new List<Guid>() {tag.Id});
                    }
                }
                Console.WriteLine();
            }
        }

        static void TrainModel()
        {
            Console.Write("Training.");
            var iteration = trainingClient.TrainProject(customVisionProject.Id);

            while(iteration.Status == "Training")
            {
                Console.Write(".");
                Thread.Sleep(5000);

                iteration = trainingClient.GetIteration(customVisionProject.Id, iteration.Id);
            }

            Console.WriteLine();
            Console.WriteLine("Model trained");
        }
    }
}