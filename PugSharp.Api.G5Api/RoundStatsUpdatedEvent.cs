namespace PugSharp.Api.G5Api;

public sealed class RoundStatsUpdatedEvent : RoundEvent
{
    public RoundStatsUpdatedEvent(string matchId, int mapNumber, int roundNumber) : base(matchId, mapNumber, roundNumber, "stats_updated")
    {
    }
}
