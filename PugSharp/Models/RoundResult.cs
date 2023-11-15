using PugSharp.Match.Contract;

namespace PugSharp.Models;

internal sealed class RoundResult : IRoundResults
{
    public required Team RoundWinner { get; init; }

    public required ITeamRoundResults TRoundResult { get; set; }

    public required ITeamRoundResults CTRoundResult { get; set; }

    public required IReadOnlyDictionary<ulong, IPlayerRoundResults> PlayerResults { get; set; }
}
