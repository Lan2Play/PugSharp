using PugSharp.Match.Contract;

namespace PugSharp.ApiStats
{
    public record RoundStatusUpdateParams(int MapNumber, ITeamInfo TeamInfo1, ITeamInfo TeamInfo2, IMap CurrentMap);
}
