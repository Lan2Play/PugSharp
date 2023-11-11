using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class DemoUploadEndedEvent : DemoFileEvent
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    public DemoUploadEndedEvent() : base("demo_upload_ended")
    {
    }
}
