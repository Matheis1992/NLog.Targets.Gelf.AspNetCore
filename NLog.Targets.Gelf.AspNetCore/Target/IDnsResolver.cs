using System.Net;

namespace NLog.Targets.Gelf.AspNetCore
{
    public abstract class DnsBase
    {
        public abstract IPAddress[] GetHostAddresses(string hostNameOrAddress);
    }

    public class DnsWrapper : DnsBase
    {

        public override IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            return Dns.GetHostAddressesAsync(hostNameOrAddress).Result;
        }
    }
}