using System.Net;

namespace NLog.Targets.Gelf.AspNetCore
{
    public interface ITransportClient
    {
        void Send(byte[] datagram, int bytes, IPEndPoint ipEndPoint);
    }
}