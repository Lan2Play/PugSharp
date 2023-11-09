using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class PreloadMatchConfigEvent : EventBase
{
    [JsonPropertyName("filename")]
    public string FileName { get; set; }

    public PreloadMatchConfigEvent(string fileName) : base("preload_match_config")
    {
        FileName = fileName;
    }
}
