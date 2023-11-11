using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class DemoFileEvent : MapEvent
{
    [JsonPropertyName("filename")]
    public required string FileName { get; init; }

    protected DemoFileEvent(string eventName) : base(eventName)
    {
    }
}
