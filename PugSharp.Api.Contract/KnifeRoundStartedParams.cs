namespace PugSharp.Api.Contract;

public class KnifeRoundStartedParams
{
    public KnifeRoundStartedParams(string matchId, int mapNumber)
    {
        MatchId = matchId;
        MapNumber = mapNumber;
    }

    public string MatchId { get; }
    public int MapNumber { get; }
}