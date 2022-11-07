using OscCore;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VRCSTT.Config;
using VRCSTT.UDT;
namespace VRCSTT.ViewModel
{
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

            if (currentSender != null)
            {
                currentSender.Dispose();
                currentSender = null;
            }

            currentSender = (new SendingObject(new CancellationTokenSource()));
            await currentSender.SendingLoop(chunks);

            currentSender = null;
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
            do
            {
                bool keepActive = VRCSTTViewModelFactory.GetInstance().KeepActive;

                if (keepActive)
                    await DoSend(1.8, chunks[0], false);
                else
                    foreach (string text in chunks)
                        await DoSend(VRCSTTViewModelFactory.GetInstance().SecondsTimer, text, text.Equals(chunks[chunks.Count - 1]));
            }
            while (VRCSTTViewModelFactory.GetInstance().KeepActive && !cts.IsCancellationRequested && !cts.Token.IsCancellationRequested);

        }

        private async Task DoSend(double waitTime, string chunk, bool last)
        {
            var message = new OscMessage("/chatbox/input", chunk, true);

            await client.SendAsync(message);
            if (!last)
                await Task.Delay((int)(waitTime * 1000));
        }

        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();
        }
    }
}
