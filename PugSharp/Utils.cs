using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace PugSharp;

// TODO Eigenen IServer um Server und Utilities ... zu wrappen
internal static class Utils
{
    public static Match.Contract.Team LoadMatchWinner()
    {
        var (CtScore, TScore) = LoadTeamsScore();
        if (CtScore > TScore)
        {
            return Match.Contract.Team.CounterTerrorist;
        }

        if (TScore > CtScore)
        {
            return Match.Contract.Team.Terrorist;
        }

        return Match.Contract.Team.None;
    }



    public static (int CtScore, int TScore) LoadTeamsScore()
    {
        var teamEntities = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
        int ctScore = 0;
        int tScore = 0;
        foreach (var team in teamEntities)
        {
            if (team.Teamname.Equals("CT", StringComparison.OrdinalIgnoreCase))
            {
                ctScore = team.Score;
            }
            else if (team.Teamname.Equals("TERRORIST", StringComparison.OrdinalIgnoreCase))
            {
                tScore = team.Score;
            }
        }

        return (ctScore, tScore);
    }
}
