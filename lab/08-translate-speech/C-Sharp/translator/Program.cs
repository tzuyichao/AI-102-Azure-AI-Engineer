using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using System.Media;

// https://learn.microsoft.com/zh-tw/azure/cognitive-services/speech-service/get-started-speech-translation?tabs=windows%2Cterminal&pivots=programming-language-csharp
// https://microsoftlearning.github.io/AI-102-AIEngineer/Instructions/08-translate-speech.html
namespace translate_client
{
    class Program
    {
        private static SpeechConfig speechConfig;
        private static SpeechTranslationConfig translationConfig;

        static async Task Main() 
        {
            try 
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcRegion = configuration["CognitiveServiceRegion"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                translationConfig = SpeechTranslationConfig.FromSubscription(cogSvcKey, cogSvcRegion);
                translationConfig.SpeechRecognitionLanguage = "en-US";
                translationConfig.AddTargetLanguage("fr");
                translationConfig.AddTargetLanguage("es");
                translationConfig.AddTargetLanguage("hi");
                Console.WriteLine("Ready to translate from " + translationConfig.SpeechRecognitionLanguage);

                speechConfig = SpeechConfig.FromSubscription(cogSvcKey, cogSvcRegion);

                string targetLanguage = "";
                while(targetLanguage != "quit")
                {
                    Console.WriteLine("\nEnter a target language\n fr = French\n es = Spanish\n hi = Hindi\n Enter anything else to stop \n");
                    targetLanguage = Console.ReadLine().ToLower();
                    if(translationConfig.TargetLanguages.Contains(targetLanguage))
                    {
                        await Translate(targetLanguage);
                    }
                    else
                    {
                        targetLanguage = "quit";
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task Translate(string targetLanguage)
        {
            string translation = "";
            string audioFile = "station.wav";
            //SoundPlayer wavPlayer = new SoundPlayer(audioFile);
            //wavPlayer.Play();

            using AudioConfig audioConfig = AudioConfig.FromWavFileInput(audioFile);
            using TranslationRecognizer transator = new TranslationRecognizer(translationConfig, audioConfig);
            Console.WriteLine("Getting speech from file...");

            TranslationRecognitionResult result = await transator.RecognizeOnceAsync();
            switch (result.Reason)
            {
                case ResultReason.TranslatedSpeech:
                    Console.WriteLine($"Translating '{result.Text}'");
                    translation = result.Translations[targetLanguage];
                    Console.OutputEncoding = Encoding.UTF8;
                    Console.WriteLine(translation);
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
            }
            
        }
    }
}