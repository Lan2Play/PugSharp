using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class KnifeRoundWonEvent : MapTeamEvent
{
    [JsonPropertyName("side_int")]
    public int Side { get; set; }

    [JsonPropertyName("swapped")]
    public bool Swapped { get; set; }

    public KnifeRoundWonEvent(string matchId, int mapNumber, int teamNumber, Side side, bool swapped) : base(matchId, mapNumber, teamNumber, "knife_won")
    {
        Side = (int)side;
        Swapped = swapped;
    }
}
