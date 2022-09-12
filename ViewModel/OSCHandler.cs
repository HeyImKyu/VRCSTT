using OscCore;
using VRCSTT.Config;
using VRCSTT.UDT;

namespace VRCSTT.ViewModel
{
    internal static class OSCHandler 
    {
        internal static void SendOverOSC(string text)
        {
            var client = new OscClient(STTConfig.Address, STTConfig.OutgoingPort);
            var message = new OscMessage("/chatbox/input", text, true);
            client.SendAsync(message);
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
