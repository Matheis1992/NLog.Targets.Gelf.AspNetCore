using System.Collections.Generic;
using System.Text.Json;
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
            var gelfJsonSer = JsonSerializer.Deserialize<Dictionary<string, object>>(gelfJson);
            Assert.Equal("value", gelfJsonSer["_test"]?.ToString());
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
            var gelfJsonSer = JsonSerializer.Deserialize<Dictionary<string, object>>(gelfJson);
            Assert.Equal("anotherValue", gelfJsonSer["_test"]?.ToString());
        }
    }
}