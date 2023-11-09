using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class DemoUploadEndedEvent : DemoFileEvent
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    public DemoUploadEndedEvent(string matchId, int mapNumber, string fileName, bool success) : base(matchId, mapNumber, fileName, "demo_upload_ended")
    {
        Success = success;
    }
}
