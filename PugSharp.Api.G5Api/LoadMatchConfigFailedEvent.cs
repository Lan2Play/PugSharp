using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class LoadMatchConfigFailedEvent : EventBase
{
    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    public LoadMatchConfigFailedEvent(string reason) : base("match_config_load_fail")
    {
        Reason = reason;
    }
}
