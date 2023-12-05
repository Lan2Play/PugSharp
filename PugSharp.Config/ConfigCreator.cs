namespace PugSharp.Config;

public class ConfigCreator
{
    public ConfigCreator()
    {
        Config = new MatchConfig
        {
            // TODO Maybe use guid for demo, ...
            MatchId = "CustomMatch",
            Team1 = new Team
            {
                Id = "1",
                Name = "Team 1",
            },
            Team2 = new Team
            {
                Id = "2",
                Name = "Team 2",
            },
            TeamMode = TeamMode.Scramble,
        };
    }

    public MatchConfig Config { get; }
}
