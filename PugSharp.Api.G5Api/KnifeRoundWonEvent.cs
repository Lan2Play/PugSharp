using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class KnifeRoundWonEvent : MapTeamEvent
{
    [JsonPropertyName("side_int")]
    public required int Side { get; init; }

    [JsonPropertyName("swapped")]
    public required bool Swapped { get; init; }

    public KnifeRoundWonEvent() : base("knife_won")
    {

    }
}
