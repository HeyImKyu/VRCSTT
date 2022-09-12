using System.Net;
using System.Net.Sockets;

namespace VRCSTT.UDT
{
    public struct UdpState
    {
        public UdpClient client;
        public IPEndPoint endpoint;
    }
}
