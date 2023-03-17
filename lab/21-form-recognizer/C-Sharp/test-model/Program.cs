using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

// import namespaces
using Azure;
using Azure.AI.FormRecognizer;  
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.Training;

namespace test_mode
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string formEndpoint = configuration["FormEndpoint"];
                string formKey = configuration["FormKey"];
                string modelId = configuration["ModelId"];

                var credential = new AzureKeyCredential(formKey);
                var recognizerClient = new FormRecognizerClient(new Uri(formEndpoint), credential);

                string imageFile = "test1.jpg";
                using(var imageData = File.OpenRead(imageFile))
                {
                    RecognizedFormCollection forms = await recognizerClient.StartRecognizeCustomForms(modelId, imageData)
                        .WaitForCompletionAsync();
                    foreach(RecognizedForm form in forms)
                    {
                        Console.WriteLine($"Form of type: {form.FormType}");
                        foreach(FormField field in form.Fields.Values)
                        {
                            Console.WriteLine($"Field '{field.Name}: ");

                            if (field.LabelData != null)
                            {
                                Console.WriteLine($"    Label: '{field.LabelData.Text}");
                            }

                            Console.WriteLine($"    Value: '{field.ValueData.Text}");
                            Console.WriteLine($"    Confidence: '{field.Confidence}");
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