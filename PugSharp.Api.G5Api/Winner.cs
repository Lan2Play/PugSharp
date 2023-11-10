using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class Winner
{
    [JsonPropertyName("side_int")]
    public int Side { get; set; }

    [JsonPropertyName("team_int")]
    public int Team { get; set; }

    public Winner(Side side, int team)
    {
        Side = (int)side;
        Team = team;
    }
}
