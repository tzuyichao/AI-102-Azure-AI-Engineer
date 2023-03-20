using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure;
using Azure.AI.AnomalyDetector;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace multivariate_example
{
    class Program
    {
        static async Task Main()
        {

        }

        private static async Task<Guid?> trainASync(AnomalyDetectorClient client, string datasource, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            try 
            {
                Console.WriteLine("Training new model...");

                int modelNumber = await GetModelNumberAsync(client, false).ConfigureAwait(false);

                ModelInfo dataFeed = new ModelInfo(datasource, startTime, endTime);

                Response<AnomalyDetectionModel> response = client.TrainMultivariateModel(dataFeed);
                response.GetRawResponse().Headers.TryGetValue("Location", out string? trainedModelIdPath);

                Guid trainedModelId = Guid.Parse(trainedModelIdPath.Split('/').LastOrDefault());
                Console.WriteLine(trainedModelId);

                modelNumber = await GetModelNumberAsync(client).ConfigureAwait(false);
                Console.WriteLine(String.Format("{0} available models after training.", modelNumber));

                return trainedModelId;
            } 
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private static async Task<int> GetModelNumberAsync(AnomalyDetectorClient client, bool delete = false)
        {
            int count = 0;

            AsyncPageable<AnomalyDetectionModel> modelList = client.GetMultivariateModelValuesAsync(0, 10000);
            await foreach(AnomalyDetectionModel model in modelList)
            {
                count += 1;
                Console.WriteLine($"Model ID: {model.ModelId}, createdTime: {model.CreatedTime}, lastUpdateTime: {model.LastUpdatedTime}");
                if(delete & count <4)
                {
                    await client.DeleteMultivariateModelAsync(model.ModelId).ConfigureAwait(false);
                }
            }

            return count;
        }
    }
}

