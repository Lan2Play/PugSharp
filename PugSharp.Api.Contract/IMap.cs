namespace PugSharp.Api.Contract;

public interface IMap
{
    string DemoFileName { get; set; }

    string Name { get; set; }

    IMapTeamInfo Team1 { get; set; }

    IMapTeamInfo Team2 { get; set; }

    public string WinnerTeamName { get; set; }
}