using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Threading;
using System.Threading.Tasks;
using VRCSTT.Config;
using VRCSTT.Helper;
using VRCSTT.UDT;

namespace VRCSTT.ViewModel
{
    internal class STTHandler
    {
        private TimeSpan timeOut = TimeSpan.FromSeconds(2);
        private System.Timers.Timer timer;
        private SpeechRecognizer speechRecognizer;

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => StopRecognition();
        private void SpeechRecognizer_SessionStopped(object? sender, SessionEventArgs e) => StopRecognition();
        private void SpeechRecognizer_Canceled(object? sender, SpeechRecognitionCanceledEventArgs e) => StopRecognition();
        private void SpeechRecognizer_Recognizing(object? sender, SpeechRecognitionEventArgs e)
        {
            this.timer.Stop();
            this.timer.Dispose();
            OSCHandler.AddToSendString(LatinizerHelper.Latinize(e.Result.Text), false);

            this.timer = new System.Timers.Timer();
            this.timer.Interval = 2000;
            this.timer.AutoReset = false;
            this.timer.Elapsed += Timer_Elapsed;
            this.timer.Start();
        }

        private void SpeechRecognizer_Recognized(object? sender, SpeechRecognitionEventArgs e)
        {
            OSCHandler.AddToSendString(LatinizerHelper.Latinize(e.Result.Text), true);

            StopRecognition();
        }

        private async Task StopRecognition()
        {
            this.timer.Dispose();

            await speechRecognizer.StopContinuousRecognitionAsync();
            speechRecognizer.Dispose();
        }
        

        internal async Task<Task> StartSpeaking(string language, Microphone microphone, bool useStandardMic, CancellationToken ct)
        {
            this.timer = new System.Timers.Timer();
            this.timer.Interval = 2000;
            this.timer.AutoReset = false;
            this.timer.Elapsed += Timer_Elapsed;
            this.timer.Start();

            if (STTConfig.SubscriptionKey == "" || STTConfig.SubscriptionKey == null || STTConfig.Region == "" || STTConfig.Region == null)
                ;

            var speechConfig = SpeechConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
            speechConfig.SpeechRecognitionLanguage = language;

            using var audioConfig = useStandardMic ? AudioConfig.FromDefaultMicrophoneInput() : AudioConfig.FromMicrophoneInput(microphone.ID);
            speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            //var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

            speechRecognizer.Recognizing += SpeechRecognizer_Recognizing;
            speechRecognizer.Recognized += SpeechRecognizer_Recognized;
            speechRecognizer.Canceled += SpeechRecognizer_Canceled;
            speechRecognizer.SessionStopped += SpeechRecognizer_SessionStopped;

            return speechRecognizer.StartContinuousRecognitionAsync();
        }


        internal async static void AbortSpeaking()
        {
            var speechConfig = SpeechConfig.FromSubscription(STTConfig.SubscriptionKey, STTConfig.Region);
            using var speechRecognizer = new SpeechRecognizer(speechConfig);

            await speechRecognizer.StopContinuousRecognitionAsync();
        }
    }
}
