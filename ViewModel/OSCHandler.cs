using OscCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRCSTT.Config;
using VRCSTT.UDT;

namespace VRCSTT.ViewModel
{
    internal static class OSCHandler 
    {
        internal static async void SendOverOSC(string text, int waitTime)
        {
            List<string> chunks = new List<string>();
            do
            {
                chunks.Add(text.Substring(0, 144));
                text.Remove(0, 144);
            }
            while (text.Length > 144);

            foreach (string chunk in chunks)
            {
                var client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
                var message = new OscMessage("/chatbox/input", chunk, true);

                await client.SendAsync(message);
                client.Dispose();

                await Task.Delay(waitTime * 1000);
            }

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
}
