using System.Net;

namespace NLog.Targets.Gelf.AspNetCore
{
    public interface ITransport
    {
        string Scheme { get; }
        void Send(IPEndPoint target, string message);
    }
}