using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Newtonsoft.Json;

namespace luis_quickstart
{
    class Program
    {
        public static async Task Main()
        {
            var key = "PASTE_YOUR_LUIS_AUTHORING_SUBSCRIPTION_KEY_HERE";
            var authoringEndpoint = "PASTE_YOUR_LUIS_AUTHORING_ENDPOINT_HERE";
            var predictionEndpoint = "PASTE_YOUR_LUIS_PREDICTION_ENDPOINT_HERE";

            var appName = "Contoso Pizza Company";
            var versionId = "0.1";
            var intentName = "OrderPizzaIntent";

            // 因為Predition那邊也有一個ApiKeyServiceClientCredentials
            var credentials = new Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.ApiKeyServiceClientCredentials(key);
            var client = new LUISAuthoringClient(credentials)
            {
                Endpoint = authoringEndpoint
            };

            var newApp = new ApplicationCreateObject
            {
                Culture = "en-us",
                Name = appName,
                InitialVersionId = versionId
            };
            var appId = await client.Apps.AddAsync(newApp);

            // create intent for app
            await client.Model.AddIntentAsync(appId, versionId, new ModelCreateObject() 
            {
                Name = intentName
            });

            // Add Prebuilt entity
            await client.Model.AddPrebuiltAsync(appId, versionId, new[] {"number"});
            
            // Define ml entity with children and grandchildren
            var mlEntityDefinition = new EntityModelCreateObject
            {
                Name = "Pizza order",
                Children = new[]
                {
                    new ChildEntityModelCreateObject
                    {
                        Name = "Pizza",
                        Children = new[]
                        {
                            new ChildEntityModelCreateObject { Name = "Quantity" },
                            new ChildEntityModelCreateObject { Name = "Type" },
                            new ChildEntityModelCreateObject { Name = "Size" }
                        }
                    },
                    new ChildEntityModelCreateObject
                    {
                        Name = "Toppings",
                        Children = new[]
                        {
                            new ChildEntityModelCreateObject { Name = "Type" },
                            new ChildEntityModelCreateObject { Name = "Quantity" }
                        }
                    }
                }
            };

            // Add ML entity
            var mlEntityId = await client.Model.AddEntityAsync(appId, versionId, mlEntityDefinition);

            // Add phraselist feature
            var phraselistId = await client.Features.AddPhraseListAsync(appId, versionId, new PhraselistCreateObject {
                EnabledForAllModels = false,
                IsExchangeable = true,
                Name = "QuantityPhraselist",
                Phrases = "few,more,extra"
            });

            // Get entity and subentities
            var model = await client.Model.GetEntityAsync(appId, versionId, mlEntityId);
            var toppingQuantityId = GetModelGrandchild(model, "Toppings", "Quantity");
            var pizzaQuantityId = GetModelGrandchild(model, "Pizza", "Quantity");

            // add model as feature to subentity model
            await client.Features.AddEntityFeatureAsync(appId, versionId, pizzaQuantityId, new ModelFeatureInformation { ModelName = "number", IsRequired = true });
            await client.Features.AddEntityFeatureAsync(appId, versionId, toppingQuantityId, new ModelFeatureInformation { ModelName = "number"});

            // add phrase list as feature to subentity model
            await client.Features.AddEntityFeatureAsync(appId, versionId, toppingQuantityId, new ModelFeatureInformation { FeatureName = "QuantityPhraselist" });

            // Define labeled example
            var labeledExampleUtteranceWithMLEntity = new ExampleLabelObject
            {
                Text = "I want two small seafood pizzas with extra cheese.",
                IntentName = intentName,
                EntityLabels = new[]
                {
                    new EntityLabelObject
                    {
                        StartCharIndex = 7,
                        EndCharIndex = 48,
                        EntityName = "Pizza order",
                        Children = new[]
                        {
                            new EntityLabelObject
                            {
                                StartCharIndex = 7,
                                EndCharIndex = 30,
                                EntityName = "Pizza",
                                Children = new[]
                                {
                                    new EntityLabelObject { StartCharIndex = 7, EndCharIndex = 9, EntityName = "Quantity" },
                                    new EntityLabelObject { StartCharIndex = 11, EndCharIndex = 15, EntityName = "Size" },
                                    new EntityLabelObject { StartCharIndex = 17, EndCharIndex = 23, EntityName = "Type" }
                                }
                            },
                            new EntityLabelObject
                            {
                                StartCharIndex = 37,
                                EndCharIndex = 48,
                                EntityName = "Toppings",
                                Children = new[]
                                {
                                    new EntityLabelObject { StartCharIndex = 37, EndCharIndex = 41, EntityName = "Quantity" },
                                    new EntityLabelObject { StartCharIndex = 43, EndCharIndex = 48, EntityName = "Type" }
                                }
                            }
                        }
                    },
                }
            };

            // Add an example for the entity.
            // Enable nested children to allow using multiple models with the same name.
            // The quantity subentity and the phraselist could have the same exact name if this is set to True
            await client.Examples.AddAsync(appId, versionId, labeledExampleUtteranceWithMLEntity, enableNestedChildren: true);

            // Train the app
            await client.Train.TrainVersionAsync(appId, versionId);
            while(true)
            {
                var status = await client.Train.GetStatusAsync(appId, versionId);
                if(status.All(m => m.Details.Status == "Success"))
                {
                    // Assumes that we never fail, and that eventually we'll always succeed.
                    break;
                }
            }

            // publish
            await client.Apps.PublishAsync(appId, new ApplicationPublishObject 
            {
                VersionId = versionId,
                IsStaging = false
            });

            Console.WriteLine("Model Published.");

            // Prediction Client
            var runtimeClient = new LUISRuntimeClient(credentials) { Endpoint = predictionEndpoint };
            
            // Production == slot name
            var request = new PredictionRequest { Query = "I want two small pepperoni pizzas with more salsa" };
            var prediction = await runtimeClient.Prediction.GetSlotPredictionAsync(appId, "Production", request);
            Console.Write(JsonConvert.SerializeObject(prediction, Formatting.Indented));

            Console.WriteLine("Done.");
        }

        static Guid GetModelGrandchild(NDepthEntityExtractor model, string childName, string grandchildName)
        {
            return model.Children
                .Single(c => c.Name == childName)
                .Children
                .Single(c => c.Name == grandchildName)
                .Id;
        }
    }
}