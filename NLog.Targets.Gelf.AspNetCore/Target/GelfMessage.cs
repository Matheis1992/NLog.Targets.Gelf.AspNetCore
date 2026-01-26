using System;
using System.Text.Json.Serialization;

namespace NLog.Targets.Gelf.AspNetCore
{
    public class GelfMessage
    {
        [JsonPropertyName("facility")]
        public string Facility { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("full_message")]
        public string FullMessage { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("line")]
        public string Line { get; set; }

        [JsonPropertyName("short_message")]
        public string ShortMessage { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}