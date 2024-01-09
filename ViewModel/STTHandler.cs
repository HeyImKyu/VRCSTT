using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
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
        internal async static Task<string> StartSpeaking(string language, Microphone microphone, bool useStandardMic, bool useTranslateHack, CancellationToken ct)
        {
            using var audioConfig = useStandardMic ? AudioConfig.FromDefaultMicrophoneInput() : AudioConfig.FromMicrophoneInput(microphone.ID);

            // Normal Speech recognition service
            if (!useTranslateHack)
            {
                if (string.IsNullOrEmpty(STTConfig.SubscriptionKey) || string.IsNullOrEmpty(STTConfig.Region))
                    return "Error: Please set SubscriptionKey and Region inside the config file!";

                var speechConfig = SpeechConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
                speechConfig.SpeechRecognitionLanguage = language;

                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                if (ct.IsCancellationRequested)
                    return "";

                speechRecognizer.Dispose();
                return OutputSpeechRecognitionResult(speechRecognitionResult);
            }
            // Speech translate service to get another 5 free hours
            else if (useTranslateHack)
            {
                var speechTranslationConfig = SpeechTranslationConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
                speechTranslationConfig.SpeechRecognitionLanguage = language;
                speechTranslationConfig.AddTargetLanguage(language);

                using var translationRecognizer = new TranslationRecognizer(speechTranslationConfig, audioConfig);

                var translationRecognitionResult = await translationRecognizer.RecognizeOnceAsync();

                if (ct.IsCancellationRequested)
                    return "";

                translationRecognizer.Dispose();
                return OutputSpeechRecognitionResult(translationRecognitionResult);
            }
            return "";
        }

        internal async static void AbortSpeaking()
        {
            if (string.IsNullOrEmpty(STTConfig.SubscriptionKey) || string.IsNullOrEmpty(STTConfig.Region))
                return;

            var speechConfig = SpeechConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
            using var speechRecognizer = new SpeechRecognizer(speechConfig);

            await speechRecognizer.StopContinuousRecognitionAsync();
        }

        private static string OutputSpeechRecognitionResult(RecognitionResult speechRecognitionResult)
        {
            string result = speechRecognitionResult.Text.Latinize();
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                case ResultReason.TranslatedSpeech:
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
