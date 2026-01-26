using System.Text.Json.Serialization;

namespace NLog.Targets.Gelf.AspNetCore
{
    public class GelfMessageV1_1
    {
        [JsonPropertyName("full_message")]
        public string FullMessage { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("short_message")]
        public string ShortMessage { get; set; }

        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}