using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class DemoFileEvent : MapEvent
{
    [JsonPropertyName("filename")]
    public string FileName { get; set; }

    protected DemoFileEvent(string matchId, int mapNumber, string fileName, string eventName) : base(matchId, mapNumber, eventName)
    {
        FileName = fileName;
    }
}
