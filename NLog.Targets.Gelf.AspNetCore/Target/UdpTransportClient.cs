using System.Net;
using System.Net.Sockets;

namespace NLog.Targets.Gelf.AspNetCore
{
    public class UdpTransportClient : ITransportClient
    {
        public void Send(byte[] datagram, int bytes, IPEndPoint ipEndPoint)
        {
            using (var udpClient = new UdpClient())
            {
                int result = udpClient.Send(datagram, bytes, ipEndPoint);
            }
        }
    }
}