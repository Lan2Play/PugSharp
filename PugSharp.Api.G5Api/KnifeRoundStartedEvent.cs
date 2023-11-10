namespace PugSharp.Api.G5Api;

public sealed class KnifeRoundStartedEvent : MapEvent
{
    public KnifeRoundStartedEvent(string matchId, int mapNumber) : base(matchId, mapNumber, "knife_start")
    {
    }
}
