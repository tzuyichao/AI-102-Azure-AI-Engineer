using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace translate_text
{
    class Program
    {
        static string translateEndpoint = "https://api.cognitive.microsofttranslator.com";
        static string cogSvcKey;
        static string cogSvcRegion;
        static async Task Main()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                cogSvcKey = configuration["CognitiveServiceKey"];
                cogSvcRegion = configuration["CognitiveServiceRegion"];

                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;

                var folderPath = Path.GetFullPath("./reviews");
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                foreach(var file in folder.GetFiles("*.txt"))
                {
                    Console.WriteLine($"\n---------------\n{file.Name}");
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    Console.WriteLine($"\n{text}");

                    string language = await GetLanguage(text);
                    Console.WriteLine($"Language: {language}");

                    if(language != "en")
                    {
                        string translatedText = await TranslateToEnglish(text, language);
                        Console.WriteLine($"\nTranslation:\n{translatedText}");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task<string> GetLanguage(string text)
        {
            string language = "en";

            object[] body = new object[] {new {Text = text}};
            var requestBody = JsonConvert.SerializeObject(body);
            Console.WriteLine($"Request Body: {requestBody}");
            using(var client = new HttpClient())
            {
                using(var request = new HttpRequestMessage())
                {
                    string path = "/detect?api-version=3.0";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translateEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    String responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    JArray jsonResponse = JArray.Parse(responseContent);
                    language = (string)jsonResponse[0]["language"];
                }
            }

            return language;
        }

        static async Task<string> TranslateToEnglish(string text, string sourceLanguage)
        {
            string translation = "";

            object[] body = new object[] {new {Text = text}};
            var requestBody = JsonConvert.SerializeObject(body);
            Console.WriteLine($"Request Body: {requestBody}");
            using(var client = new HttpClient())
            {
                using(var request = new HttpRequestMessage())
                {
                    string path = "/translate?api-version=3.0&from=" + sourceLanguage + "&to=en";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translateEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    String responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    JArray jsonResponse = JArray.Parse(responseContent);
                    translation = (string)jsonResponse[0]["translations"][0]["text"];
                }
            }

            return translation;
        }
    }
}