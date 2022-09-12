using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRCSTT.Config;
using VRCSTT.Helper;
using VRCSTT.UDT;

namespace VRCSTT.ViewModel
{
    internal static class STTHandler
    {
        internal async static Task<string> StartSpeaking(string language, Microphone microphone, bool useStandardMic, CancellationToken ct)
        {
            if (STTConfig.SubscriptionKey == "" || STTConfig.SubscriptionKey == null || STTConfig.Region == "" || STTConfig.Region == null)
                return "Error: Please set SubscriptionKey and Region inside the config file!";

            var speechConfig = SpeechConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
            speechConfig.SpeechRecognitionLanguage = language;

            using var audioConfig = useStandardMic ? AudioConfig.FromDefaultMicrophoneInput() : AudioConfig.FromMicrophoneInput(microphone.ID);
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

            if (ct.IsCancellationRequested)
                return "";

            speechRecognizer.Dispose();
            return OutputSpeechRecognitionResult(speechRecognitionResult);
        }

        internal async static void AbortSpeaking()
        {
            var speechConfig = SpeechConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
            using var speechRecognizer = new SpeechRecognizer(speechConfig);

            await speechRecognizer.StopContinuousRecognitionAsync();
        }

        private static string OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            string result = speechRecognitionResult.Text.Latinize();
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    return result;
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
