using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class EventBase
{
    public EventBase(string eventName)
    {
        EventName = eventName;
    }

    [JsonPropertyName("event")]
    public string EventName { get; }
}
