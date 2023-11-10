using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class TeamWrapper
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    public TeamWrapper(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
