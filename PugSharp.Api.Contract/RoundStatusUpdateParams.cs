namespace PugSharp.Api.Contract;

public record RoundStatusUpdateParams(string MatchId, int MapNumber, ITeamInfo TeamInfo1, ITeamInfo TeamInfo2, IMap CurrentMap);
