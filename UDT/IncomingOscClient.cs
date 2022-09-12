using System;
using System.Net;
using System.Net.Sockets;

namespace VRCSTT.UDT
{
    internal class IncomingOscClient
    {
        private readonly UdpState state;
        private readonly AsyncCallback callback;

        public IncomingOscClient(int port, AsyncCallback? callback)
        {
            this.callback = callback;
            IPEndPoint e = new IPEndPoint(IPAddress.Any, port);
            var c = new UdpClient(e);

            state = new UdpState();
            state.client = c;
            state.endpoint = e;

            state.client.BeginReceive(this.callback, state);
        }

        internal void BeginReceiving()
        {
            state.client.BeginReceive(this.callback, state);
        }
    }
}
