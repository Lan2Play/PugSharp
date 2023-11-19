namespace PugSharp.Api.Contract;

public interface IMap
{
    string DemoFileName { get; }

    string Name { get; }

    IMapTeamInfo Team1 { get; }

    IMapTeamInfo Team2 { get; }

    public string WinnerTeamName { get; }
}