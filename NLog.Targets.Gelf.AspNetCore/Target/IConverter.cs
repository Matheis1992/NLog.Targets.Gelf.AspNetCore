using System;

namespace NLog.Targets.Gelf.AspNetCore
{
    public interface IConverter
    {
        string GetGelfJson(LogEventInfo logEventInfo, string facility, string gelfVersion = "1.0");
    }
}