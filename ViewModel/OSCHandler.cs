using OscCore;
using System.IO;
using VRCSTT.Config;
using VRCSTT.UDT;
using System.Timers;
using System;

namespace VRCSTT.ViewModel
{
    internal static class OSCHandler 
    {
        internal static DateTime lastSent;

        internal static OscClient client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
        internal static void SendOverOSC(string text)
        {
            //var client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
            var message = new OscMessage("/chatbox/input", text, true);
            var log = client.SendAsync(message);

            //IsTyping(false);
        }

        internal static void IsTyping(bool b)
        {
            var client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
            var message = new OscMessage("/chatbox/typing", b); 
            client.SendAsync(message);
            client.Dispose();
        }

        internal static void AddToSendString(string text, bool last)
        {
            if (!last && !CanSend())
                return;

            if (text.Length > 144)
                text = text.Substring(text.Length - 144, 144);

            SendOverOSC(text);
        }

        private static bool CanSend()
        {
            if (lastSent == null)
            {
                lastSent = DateTime.Now;
            }

            if (DateTime.Now.Subtract(lastSent) < TimeSpan.FromMilliseconds(1500))
            {
                return false;
            }

            lastSent = DateTime.Now;
            return true;
        }
    }
}
