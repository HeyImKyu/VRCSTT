using OscCore;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VRCSTT.UDT
{
    internal class OscClient
    {
        private readonly UdpClient client;
        private readonly CancellationTokenSource cts;

        public event EventHandler<OscPacket> PacketReceived;

        public OscClient(string hostName, int port)
        {
            client = new UdpClient();
            client.Connect(hostName, port);
            cts = new CancellationTokenSource();
        }

        public Task SendAsync(OscPacket packet)
        {
            var data = packet.ToByteArray();
            return client.SendAsync(data, data.Length);
        }

        public void Dispose() => cts.Cancel();
    }
}
