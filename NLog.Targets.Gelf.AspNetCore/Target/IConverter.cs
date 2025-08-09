using Newtonsoft.Json.Linq;

namespace NLog.Targets.Gelf.AspNetCore
{
    public interface IConverter
    {
        JObject GetGelfJson(LogEventInfo logEventInfo, string facility, string gelfVersion = "1.0");
    }
}