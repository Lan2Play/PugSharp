using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class RoundMvpEvent : PlayerRoundEvent
{
    [JsonPropertyName("reason")]
    public required int Reason { get; init; }

    public RoundMvpEvent() : base("round_mvp")
    {
    }
}
