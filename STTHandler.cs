using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCSTT.Config;

namespace VRCSTT
{
    internal static class STTHandler
    {
        internal static string StartSpeaking(string language)
        {
            if (STTConfig.SubscriptionKey == "" || STTConfig.SubscriptionKey == null || STTConfig.Region == "" || STTConfig.Region == null)
                return "Error: Please set SubscriptionKey and Region inside the config file!";

            var speechConfig = SpeechConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
            speechConfig.SpeechRecognitionLanguage = language;

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            var speechRecognitionResult = speechRecognizer.RecognizeOnceAsync().Result;
            return OutputSpeechRecognitionResult(speechRecognitionResult);
        }

        private static string OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    OSCHandler.SendOverOSC(speechRecognitionResult.Text);
                    return speechRecognitionResult.Text;
                case ResultReason.NoMatch:
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
            }
            return "";
        }
    }
}
