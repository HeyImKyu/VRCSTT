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
                chunks.Add(text.Substring(0, text.Length > 144 ? 144 : text.Length));
                text = text.Remove(0, text.Length > 144 ? 144 : text.Length);
            }
            while (text.Length > 0);

            var client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
            foreach (string chunk in chunks)
            {
                var message = new OscMessage("/chatbox/input", chunk, true);

                await client.SendAsync(message);
                await Task.Delay(waitTime * 1000);
            }
            client.Dispose();

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
