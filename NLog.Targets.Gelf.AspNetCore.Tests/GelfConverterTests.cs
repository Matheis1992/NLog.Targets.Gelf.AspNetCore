using Xunit;

namespace NLog.Targets.Gelf.AspNetCore.Tests
{
    public class GelfConverterTests
    {
        [Fact]
        public void ShouldGetGelfJsonAddMappedDiagnosticsLogicalContextData()
        {
            ScopeContext.PushProperty("test", "value");

            var logEvent = LogEventInfo.Create(LogLevel.Info, "loggerName", null, "message");

            var converter = new GelfConverter();

            // Act
            var gelfJson = converter.GetGelfJson(logEvent, "facility");

            Assert.Equal("value", gelfJson.Value<string>("_test"));
        }

        [Fact]
        public void ShouldGetGelfJsonDiscardMappedDiagnosticsLogicalContextDataIfPresentInLogEventInfo()
        {
            ScopeContext.PushProperty("test", "value");
            
            var logEvent = LogEventInfo.Create(LogLevel.Info, "loggerName", null, "message");
            logEvent.Properties.Add("test", "anotherValue");

            var converter = new GelfConverter();

            // Act
            var gelfJson = converter.GetGelfJson(logEvent, "facility");

            Assert.Equal("anotherValue", gelfJson.Value<string>("_test"));
        }
    }
}