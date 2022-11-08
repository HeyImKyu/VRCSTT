using OscCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRCSTT.Config;
using VRCSTT.Helper;
using VRCSTT.UDT;

namespace VRCSTT.ViewModel
{
    internal enum DisplayTypes
    {
        /// <summary>
        /// Display a Message or multiple Messages once
        /// </summary>
        Once,

        /// <summary>
        /// Display a single Message and keep it displayed
        /// </summary>
        Keep,

        /// <summary>
        /// Keep displayed multiple messages
        /// Cycles through them
        /// </summary>
        CycleMultiple,

        /// <summary>
        /// Used when displaying the currently playing song
        /// </summary>
        NowPlaying
    }

    internal static class OSCHandler
    {
        private static SendingObject? currentSender;

        internal static async void SendOverOSC(string text, int waitTime)
        {
            List<string> chunks = new List<string>();
            do
            {
                chunks.Add(text.Substring(0, text.Length > 144 ? 144 : text.Length));
                text = text.Remove(0, text.Length > 144 ? 144 : text.Length);
            }
            while (text.Length > 0);

            currentSender?.Dispose();

            currentSender = (new SendingObject(new CancellationTokenSource()));
            await currentSender.SendingLoop(chunks);
            IsTyping(false);
        }
        internal static void IsTyping(bool b)
        {
            var client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
            var message = new OscMessage("/chatbox/typing", b);
            client.SendAsync(message);
            client.Dispose();
        }
    }

    internal class SendingObject : IDisposable
    {
        private readonly CancellationTokenSource cts;
        private readonly OscClient client;
        internal SendingObject(CancellationTokenSource cts)
        {
            this.cts = cts;
            this.client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
        }
        internal async Task SendingLoop(List<string> chunks)
        {
            DisplayTypes displayType = DisplayTypes.Once;
            do
            {
                bool keepActive = VRCSTTViewModelFactory.GetInstance().KeepActive;
                if (VRCSTTViewModelFactory.GetInstance().CurrentSong)
                    displayType = DisplayTypes.NowPlaying;
                else if (keepActive && chunks.Count > 1)
                    displayType = DisplayTypes.CycleMultiple;
                else if (keepActive)
                    await DoSend(1.8, chunks[0], false);
                else
                    displayType = DisplayTypes.Once;
                switch (displayType)
                {
                    case DisplayTypes.Once:
                        foreach (string text in chunks)
                            await DoSend(VRCSTTViewModelFactory.GetInstance().SecondsTimer, text, text.Equals(chunks[chunks.Count - 1]));
                        break;
                    case DisplayTypes.Keep:
                        await DoSend(1.8, chunks[0], false);
                        break;
                    case DisplayTypes.CycleMultiple:
                        foreach (string text in chunks)
                            await DoSend(VRCSTTViewModelFactory.GetInstance().SecondsTimer, text, false);
                        break;
                    case DisplayTypes.NowPlaying:
                        var musicInfo = await MusicHandler.GetMusicInformation();
                        var formatted = await MusicHandler.FormatInformation(musicInfo);

                        formatted = formatted.Latinize();
                        VRCSTTViewModelFactory.GetInstance().TextboxText = formatted;
                        await DoSend(1.8, formatted, false);
                        break;
                }
            }
            while (
            (VRCSTTViewModelFactory.GetInstance().KeepActive || VRCSTTViewModelFactory.GetInstance().CurrentSong)
            &&
            (!cts.IsCancellationRequested && !cts.Token.IsCancellationRequested)
            );
        }
        private async Task DoSend(double waitTime, string chunk, bool last)
        {
            var message = new OscMessage("/chatbox/input", chunk, true);
            try
            {
                if (!this.cts.Token.IsCancellationRequested && !this.cts.IsCancellationRequested)
                    await client.SendAsync(message);
            }
            catch (Exception)
            {
                // ObjectDisposedException
            }
            if (!last)
            {
                try
                {
                    await Task.Delay((int)(waitTime * 1000), this.cts.Token);
                }
                catch (Exception)
                {
                    // OperationCanceledException
                    // ObjectDisposedException
                }
            }
        }

        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();
            this.client.Dispose();
        }
    }
}