namespace PugSharp.Api.Contract;

public class KnifeRoundWonParams
{
    public KnifeRoundWonParams(string matchId, int mapNumber, int winningSide, bool swapped)
    {
        MatchId = matchId;
        MapNumber = mapNumber;
        WinningSide = winningSide;
        Swapped = swapped;
    }

    public string MatchId { get; }
    public int MapNumber { get; }
    public int WinningSide { get; }
    public bool Swapped { get; }
}