namespace PugSharp.Api.Contract;

public record RoundMvpParams(string MatchId, int MapNumber, int RoundNumber, IApiPlayer Player, int Reason);