using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class BackupRestoredEvent : RoundEvent
{
    [JsonPropertyName("filename")]
    public required string FileName { get; init; }

    public BackupRestoredEvent() : base("backup_loaded")
    {
    }
}
