using PugSharp.Api.Contract;

namespace PugSharp.ApiStats;

public class Map : IMap
{
    public required string Name { get; init; }

    public required string DemoFileName { get; init; }

    public required string WinnerTeamName { get; init; }

    public required TeamSide WinnerTeamSide { get; init; }

    public required IMapTeamInfo Team1 { get; init; }

    public required IMapTeamInfo Team2 { get; init; }
}
