﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Language.Conversations;

namespace clock_client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfiguration configuration = builder.Build();
            string predictionEndpoint = configuration["LSPredictionEndpoint"];
            string predictionKey = configuration["LSPredictionKey"];

            Uri lsEndpoint = new Uri(predictionEndpoint);
            AzureKeyCredential credential = new AzureKeyCredential(predictionKey);
            ConversationAnalysisClient client = new ConversationAnalysisClient(lsEndpoint, credential);

            var lsProject = "Clock";
            var lsSlot = "production";
            ConversationsProject conversationsProject = new ConversationsProject(lsProject, lsSlot);
            string userText = "";
            while(userText.ToLower() != "quit")
            {
                Console.WriteLine("\nEnter some text ('quit' to quit)");
                userText = Console.ReadLine();
                if(userText.ToLower() != "quit")
                {
                    Response<AnalyzeConversationResult> response = await client.AnalyzeConversationAsync(userText, conversationsProject);
                    ConversationPrediction conversationPrediction = response.Value.Prediction as ConversationPrediction;

                    Console.WriteLine(JsonConvert.SerializeObject(conversationPrediction, Formatting.Indented));
                    Console.WriteLine("-------------------\n");
                    Console.WriteLine(userText);

                    var topIntent = "";
                    if(conversationPrediction.Intents[0].Confidence > 0.5)
                    {
                        topIntent = conversationPrediction.TopIntent;
                    }
                    var entities = conversationPrediction.Entities;
                    switch(topIntent)
                    {
                        case "GetTime":
                            var location = "local";
                            // Check for entities
                            if (entities.Count > 0)
                            {
                                // Check for a location entity
                                foreach (ConversationEntity entity in conversationPrediction.Entities)
                                {
                                    if (entity.Category == "Location")
                                    {
                                        //Console.WriteLine($"Location Confidence: {entity.Confidence}");
                                        location = entity.Text;
                                    }
                                        
                                }

                            }

                            // Get the time for the specified location
                            var getTimeTask = Task.Run(() => GetTime(location));
                            string timeResponse = await getTimeTask;
                            Console.WriteLine(timeResponse);
                            break;
                        case "GetDay":
                            var date = DateTime.Today.ToShortDateString();
                            // Check for entities
                            if (entities.Count > 0)
                            {
                                // Check for a Date entity
                                foreach (ConversationEntity entity in conversationPrediction.Entities)
                                {
                                    if (entity.Category == "Date")
                                    {
                                        //Console.WriteLine($"Location Confidence: {entity.Confidence}");
                                        date = entity.Text;
                                    }
                                }
                            }
                            // Get the day for the specified date
                            var getDayTask = Task.Run(() => GetDay(date));
                            string dayResponse = await getDayTask;
                            Console.WriteLine(dayResponse);
                            break;
                        case "GetDate":
                            var day = DateTime.Today.DayOfWeek.ToString();
                            // Check for entities
                            if (entities.Count > 0)
                            {
                                // Check for a Weekday entity
                                foreach (ConversationEntity entity in conversationPrediction.Entities)
                                {
                                    if (entity.Category == "Weekday")
                                    {
                                        //Console.WriteLine($"Location Confidence: {entity.Confidence}");
                                        day = entity.Text;
                                    }
                                }
                                    
                            }
                            // Get the date for the specified day
                            var getDateTask = Task.Run(() => GetDate(day));
                            string dateResponse = await getDateTask;
                            Console.WriteLine(dateResponse);
                            break;
                        default:
                            Console.WriteLine("Try asking me for the time, the day, or the date.");
                            break;
                    }
                }
            }
        }

        // helpers 雖然手打可以熟悉一下.net內建library使用，節省時間直接貼
        static string GetTime(string location)
        {
            var timeString = "";
            var time = DateTime.Now;

            /* Note: To keep things simple, we'll ignore daylight savings time and support only a few cities.
               In a real app, you'd likely use a web service API (or write  more complex code!)
               Hopefully this simplified example is enough to get the the idea that you
               use LU to determine the intent and entities, then implement the appropriate logic */

            switch (location.ToLower())
            {
                case "local":
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "london":
                    time = DateTime.UtcNow;
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "sydney":
                    time = DateTime.UtcNow.AddHours(11);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "new york":
                    time = DateTime.UtcNow.AddHours(-5);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "nairobi":
                    time = DateTime.UtcNow.AddHours(3);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "tokyo":
                    time = DateTime.UtcNow.AddHours(9);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "delhi":
                    time = DateTime.UtcNow.AddHours(5.5);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                default:
                    timeString = "I don't know what time it is in " + location;
                    break;
            }

            return timeString;
        }

        static string GetDate(string day)
        {
            string date_string = "I can only determine dates for today or named days of the week.";

            // To keep things simple, assume the named day is in the current week (Sunday to Saturday)
            DayOfWeek weekDay;
            if (Enum.TryParse(day, true, out weekDay))
            {
                int weekDayNum = (int)weekDay;
                int todayNum = (int)DateTime.Today.DayOfWeek;
                int offset = weekDayNum - todayNum;
                date_string = DateTime.Today.AddDays(offset).ToShortDateString();
            }
            return date_string;

        }

        static string GetDay(string date)
        {
            // Note: To keep things simple, dates must be entered in US format (MM/DD/YYYY)
            string day_string = "Enter a date in MM/DD/YYYY format.";
            DateTime dateTime;
            if (DateTime.TryParse(date, out dateTime))
            {
                day_string = dateTime.DayOfWeek.ToString();
            }

            return day_string;
        }
    }
}