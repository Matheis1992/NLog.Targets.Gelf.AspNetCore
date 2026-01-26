using Moq;
using Newtonsoft.Json.Linq;
using NLog.Targets.Gelf.AspNetCore.Tests.Resources;
using System;
using System.Net;
using Xunit;

namespace NLog.Targets.Gelf.AspNetCore.Tests
{
    public class UdpTransportTest
    {
        public class SendMethod
        {
            [Fact]
            public void ShouldSendShortUdpMessage()
            {
                var transportClient = new Mock<ITransportClient>();
                var transport = new UdpTransport(transportClient.Object);
                var converter = new Mock<IConverter>();
                var dnslookup = new Mock<DnsBase>();
                converter.Setup(c => c.GetGelfJson(It.IsAny<LogEventInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new JObject().ToString());

                var target = new GelfTarget { Endpoint = "udp://192.168.99.100:12201" };
                var logEventInfo = new LogEventInfo { Message = "Test Message" };
                dnslookup.Setup(x => x.GetHostAddresses(It.IsAny<string>())).Returns([IPAddress.Parse("192.168.99.100")]);

                target.WriteLogEventInfo(logEventInfo);

                transportClient.Verify(t => t.Send(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IPEndPoint>()), Times.Once());
                converter.Verify(c => c.GetGelfJson(It.IsAny<LogEventInfo>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            }

            [Fact]
            public void ShouldSendLongUdpMessage()
            {
                var jsonObject = new JObject();
                var message = ResourceHelper.GetResource("LongMessage.txt").ReadToEnd();

                jsonObject.Add("full_message", JToken.FromObject(message));

                var converter = new Mock<IConverter>();
                converter.Setup(c => c.GetGelfJson(It.IsAny<LogEventInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(jsonObject.ToString()).Verifiable();
                var transportClient = new Mock<ITransportClient>();
                transportClient.Setup(t => t.Send(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IPEndPoint>())).Verifiable();

                var transport = new UdpTransport(transportClient.Object);
                var dnslookup = new Mock<DnsBase>();
                dnslookup.Setup(x => x.GetHostAddresses(It.IsAny<string>())).Returns([IPAddress.Parse("192.168.99.100")]);
                var target = new GelfTarget { Endpoint = "udp://192.168.99.100:12201" };
                target.WriteLogEventInfo(new LogEventInfo());

                converter.Verify(c => c.GetGelfJson(It.IsAny<LogEventInfo>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            }
        }
    }
}