using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class PlayerBase
{
    [JsonPropertyName("steamid")]
    public string SteamId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    protected PlayerBase(string steamId, string name)
    {
        SteamId = steamId;
        Name = name;
    }
}
