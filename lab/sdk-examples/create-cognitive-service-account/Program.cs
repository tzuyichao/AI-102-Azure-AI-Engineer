using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.CognitiveServices;
using Microsoft.Azure.Management.CognitiveServices.Models;

namespace azure_management_quickstart
{
    class Program
    {
        static void Main()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            var clientId = configuration["ApplicationID"];
            var clientSecret = configuration["ClientSecret"];
            var subId = configuration["SubscriptionID"];
            var tenantId = configuration["TenantID"];
            var resourceGroupName = configuration["ResourceGroupName"];

            var servicePrincipalCredentials = new ServicePrincipalLoginInformation();
            servicePrincipalCredentials.ClientId = clientId;
            servicePrincipalCredentials.ClientSecret = clientSecret;

            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
            var client = new CognitiveServicesManagementClient(credentials);
            client.SubscriptionId = subId;

            create_resource(client, resourceGroupName, "test20230329", "CognitiveServices", "F0", "westus");
        }

        static void create_resource(CognitiveServicesManagementClient client, string resourceGroupName, string resourceName, string kind, string accountTier, string location)
        {
            Console.WriteLine($"Creating resource: {resourceName}...");
            Account account = new Account();
            account.Kind = kind;
            account.Location = location;
            account.Sku = new Sku(accountTier);

            var result = client.Accounts.Create(resourceGroupName, resourceGroupName, account);
            Console.WriteLine("Resource created.");
            Console.WriteLine($"ID: {result.Id}");
            Console.WriteLine($"Kind: {result.Kind}");
            Console.WriteLine();
        }
    }
}