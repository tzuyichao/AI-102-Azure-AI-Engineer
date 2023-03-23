﻿using System.Text.Json;
using Azure;
using Azure.AI.Language.Conversations;
using Azure.Core;
using Newtonsoft.Json;

namespace get_start
{
    class Program
    {
        static void Main()
        {
            Uri endpoint = new Uri("endpoint");
            AzureKeyCredential credential = new AzureKeyCredential("apikey");

            ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credential);
            var data = new
            {
                analysisInput = new
                {
                    conversations = new[]
                    {
                        new
                        {
                            conversationItems = new[]
                            {
                                new
                                {
                                    itn = "hi",
                                    maskedItn = "hi",
                                    text = "Hi",
                                    lexical = "hi",
                                    audioTimings = new[]
                                    {
                                        new
                                        {
                                            word = "hi",
                                            offset = 4500000,
                                            duration = 2800000,
                                        },
                                    },
                                    id = "1",
                                    participantId = "speaker",
                                },
                                new
                                {
                                    itn = "jane doe",
                                    maskedItn = "jane doe",
                                    text = "Jane Doe",
                                    lexical = "jane doe",
                                    audioTimings = new[]
                                    {
                                        new
                                        {
                                            word = "jane",
                                            offset = 7100000,
                                            duration = 4800000,
                                        },
                                        new
                                        {
                                            word = "doe",
                                            offset = 12000000,
                                            duration = 1700000,
                                        },
                                    },
                                    id = "3",
                                    participantId = "agent",
                                },
                                new
                                {
                                    itn = "hi jane what's your phone number",
                                    maskedItn = "hi jane what's your phone number",
                                    text = "Hi Jane, what's your phone number?",
                                    lexical = "hi jane what's your phone number",
                                    audioTimings = new[]
                                    {
                                        new
                                        {
                                        word = "hi",
                                        offset = 7700000,
                                        duration= 3100000,
                                        },
                                        new
                                        {
                                        word= "jane",
                                        offset= 10900000,
                                        duration= 5700000,
                                        },
                                        new
                                        {
                                        word= "what's",
                                        offset= 17300000,
                                        duration= 2600000,
                                        },
                                        new
                                        {
                                        word= "your",
                                        offset= 20000000,
                                        duration= 1600000,
                                        },
                                        new
                                        {
                                        word= "phone",
                                        offset= 21700000,
                                        duration= 1700000,
                                        },
                                        new
                                        {
                                        word= "number",
                                        offset= 23500000,
                                        duration= 2300000,
                                        },
                                    },
                                    id = "2",
                                    participantId = "speaker",
                                },
                            },
                            id = "1",
                            language = "en",
                            modality = "transcript",
                        },
                    }
                },
                tasks = new[]
                {
                    new
                    {
                        taskName = "analyze",
                        kind = "ConversationalPIITask",
                        parameters = new
                        {
                            piiCategories = new[]
                            {
                                "All",
                            },
                            includeAudioRedaction = false,
                            redactionSource = "lexical",
                            modelVersion = "2022-05-15-preview",
                            loggingOptOut = false,
                        },
                    },
                },
            };

            Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));
            Response response = client.AnalyzeConversation(RequestContent.Create(data));
            
            using JsonDocument result = JsonDocument.Parse(response.ContentStream);
            JsonElement jobResults = result.RootElement;
            foreach (JsonElement task in jobResults.GetProperty("tasks").GetProperty("items").EnumerateArray())
            {
                JsonElement results = task.GetProperty("results");

                Console.WriteLine("Conversations:");
                foreach (JsonElement conversation in results.GetProperty("conversations").EnumerateArray())
                {
                    Console.WriteLine($"Conversation: #{conversation.GetProperty("id").GetString()}");
                    Console.WriteLine("Conversation Items:");
                    foreach (JsonElement conversationItem in conversation.GetProperty("conversationItems").EnumerateArray())
                    {
                        Console.WriteLine($"Conversation Item: #{conversationItem.GetProperty("id").GetString()}");

                        JsonElement redactedContent = conversationItem.GetProperty("redactedContent");
                        Console.WriteLine($"Redacted Text: {redactedContent.GetProperty("text").GetString()}");
                        Console.WriteLine($"Redacted Lexical: {redactedContent.GetProperty("lexical").GetString()}");
                        Console.WriteLine($"Redacted MaskedItn: {redactedContent.GetProperty("maskedItn").GetString()}");

                        Console.WriteLine("Entities:");
                        foreach (JsonElement entity in conversationItem.GetProperty("entities").EnumerateArray())
                        {
                            Console.WriteLine($"Text: {entity.GetProperty("text").GetString()}");
                            Console.WriteLine($"Offset: {entity.GetProperty("offset").GetInt32()}");
                            Console.WriteLine($"Category: {entity.GetProperty("category").GetString()}");
                            Console.WriteLine($"Confidence Score: {entity.GetProperty("confidenceScore").GetSingle()}");
                            Console.WriteLine($"Length: {entity.GetProperty("length").GetInt32()}");
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}