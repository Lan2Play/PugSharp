using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class BackupRestoredEvent : RoundEvent
{
    [JsonPropertyName("filename")]
    public string FileName { get; set; }

    public BackupRestoredEvent(string matchId, int mapNumber, int roundNumber, string fileName) : base(matchId, mapNumber, roundNumber, "backup_loaded")
    {
        FileName = fileName;
    }
}
