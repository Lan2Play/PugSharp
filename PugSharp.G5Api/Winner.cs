using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class Winner
{
    [JsonPropertyName("side_int")]
    public int Side { get; set; }

    [JsonPropertyName("team_int")]
    public int Team { get; set; }

    public Winner(int side, int team)
    {
        Side = side;
        Team = team;
    }
}
