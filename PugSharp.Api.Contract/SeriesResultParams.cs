namespace PugSharp.Api.Contract
{
    public record SeriesResultParams(string MatchId, string WinnerTeamName, bool Forfeit, uint TimeBeforeFreeingServerMs, int Team1SeriesScore, int Team2SeriesScore);
}
