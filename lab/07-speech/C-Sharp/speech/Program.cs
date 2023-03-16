using System;
using System.IO;
using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;

// https://microsoftlearning.github.io/AI-102-AIEngineer/Instructions/07-speech.html
namespace speech_client
{
    class Program
    {
        private static SpeechConfig speechConfig;
        static async Task Main() 
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcRegion = configuration["CognitiveServicesRegion"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                speechConfig = SpeechConfig.FromSubscription(cogSvcKey, cogSvcRegion);
                Console.WriteLine("Ready to use speech service in " + speechConfig.Region);
                speechConfig.SpeechSynthesisVoiceName = "en-US-AriaNeural";

                string command = "";
                command = await TranscribeCommand();
                if(command.ToLower() == "what time is it?")
                {
                    await TellTime();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task<string> TranscribeCommand()
        {
            string command = "";
            // From Microphone Input
            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            Console.WriteLine("Speak now ...");

            // Process speech input
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            if(speech.Reason == ResultReason.RecognizedSpeech)
            {
                command = speech.Text;
                Console.WriteLine($"Recognition Result: {command}");
            }
            else
            {
                Console.WriteLine(speech.Reason);
                if(speech.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.Reason);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }

            return command;
        }

        static async Task TellTime()
        {
            Console.WriteLine("Telling...");
            var now = DateTime.Now;
            string responseText = "The time is " + now.Hour.ToString() + ":" + now.Minute.ToString("D2");
            using AudioConfig audioConfig = AudioConfig.FromDefaultSpeakerOutput();
            speechConfig.SpeechSynthesisVoiceName = "en-GB-AbbiNeural";
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

            SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(responseText);
            if(speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine(speak.Reason);
            }

            Console.WriteLine(responseText);
        }
    }
}