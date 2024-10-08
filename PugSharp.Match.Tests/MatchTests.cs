using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using PugSharp.Api.Contract;
using PugSharp.Config;
using PugSharp.Match.Contract;
using PugSharp.Server.Contract;
using PugSharp.Translation;

namespace PugSharp.Match.Tests;

public class MatchTests
{
    private static ServiceProvider CreateTestProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<ICsServer>());
        services.AddSingleton<IApiProvider, MultiApiProvider>();

        services.AddSingleton(Substitute.For<ICssDispatcher>());
        services.AddSingleton(Substitute.For<ICsServer>());
        services.AddLogging(options =>
        {
            //options.AddConsole();
        });
        services.AddSingleton<IApplication, Application>();

        services.AddSingleton<ConfigProvider>();
        services.AddTransient<MatchFactory>();
        services.AddSingleton(Substitute.For<ITextHelper>());

        // Build service provider
        return services.BuildServiceProvider();
    }

    [Fact]
    public void CreateDotGraphTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();
        var matchFactory = serviceProvider.GetRequiredService<MatchFactory>();
        var match = matchFactory.CreateMatch(config);

        var dotGraphString = match.CreateDotGraph();
        Assert.False(string.IsNullOrEmpty(dotGraphString));
    }

    [Fact]
    public void WrongPlayerConnectTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();
        var matchFactory = serviceProvider.GetRequiredService<MatchFactory>();
        var match = matchFactory.CreateMatch(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player = CreatePlayerSub(1337, 0);

        Assert.False(match.TryAddPlayer(player));
    }

    [Fact]
    public void CorrectPlayerConnectTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();
        var matchFactory = serviceProvider.GetRequiredService<MatchFactory>();
        var match = matchFactory.CreateMatch(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player = CreatePlayerSub(0, 0);

        Assert.True(match.TryAddPlayer(player));
    }

    [Fact]
    public void MatchTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();

        var matchPlayers = new List<IPlayer>();
        var csServer = serviceProvider.GetRequiredService<ICsServer>();
        csServer.LoadAllPlayers().Returns(matchPlayers);

        var matchFactory = serviceProvider.GetRequiredService<MatchFactory>();
        var match = matchFactory.CreateMatch(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player1 = CreatePlayerSub(0, 0);
        IPlayer player2 = CreatePlayerSub(1, 1);

        ConnectPlayers(matchPlayers, match, player1, player2);
        SetPlayersReady(match, player1, player2, MatchState.MapVote);
        IPlayer votePlayer = VoteForMap(config, match, player1, player2);
        VoteTeam(csServer, config, match, player1, player2, votePlayer);
        PauseUnpauseMatch(csServer, match, player1);
    }

    [Fact]
    public void MatchTestWithOneMap()
    {
        MatchConfig config = CreateExampleConfig(new List<string> { "de_dust2" });
        var serviceProvider = CreateTestProvider();

        var matchPlayers = new List<IPlayer>();
        var csServer = serviceProvider.GetRequiredService<ICsServer>();
        csServer.LoadAllPlayers().Returns(matchPlayers);

        var matchFactory = serviceProvider.GetRequiredService<MatchFactory>();
        var match = matchFactory.CreateMatch(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player1 = CreatePlayerSub(0, 0);
        IPlayer player2 = CreatePlayerSub(1, 1);

        ConnectPlayers(matchPlayers, match, player1, player2);
        SetPlayersReady(match, player1, player2, MatchState.TeamVote);
        VoteTeam(csServer, config, match, player1, player2, player1);
        PauseUnpauseMatch(csServer, match, player1);
    }


    private static void PauseUnpauseMatch(ICsServer csServer, Match match, IPlayer player1)
    {
        match.SetPlayerDisconnected(player1);
        Assert.Equal(MatchState.MatchPaused, match.CurrentState);
        csServer.Received().PauseMatch();

        match.TryAddPlayer(player1);
        Assert.Equal(MatchState.MatchRunning, match.CurrentState);
        csServer.Received().UnpauseMatch();
    }

    private static void VoteTeam(ICsServer csServer, MatchConfig config, Match match, IPlayer player1, IPlayer player2, IPlayer votePlayer)
    {
        Assert.False(match.VoteTeam(votePlayer, "pizza"));
        Assert.False(match.VoteTeam(votePlayer == player1 ? player2 : player1, "T"));
        Assert.True(match.VoteTeam(votePlayer, "T"));

        csServer.Received().SwitchMap(config.Maplist[^1]);

        Assert.Equal(MatchState.WaitingForPlayersReady, match.CurrentState);
        match.TogglePlayerIsReady(player1);
        Assert.Equal(MatchState.WaitingForPlayersReady, match.CurrentState);
        match.TogglePlayerIsReady(player2);

        Assert.Equal(MatchState.MatchRunning, match.CurrentState);
    }

    private static IPlayer VoteForMap(MatchConfig config, Match match, IPlayer player1, IPlayer player2)
    {
        var matchCount = config.Maplist.Count;
        var votePlayer = player1;

        Assert.False(match.BanMap(votePlayer, matchCount));

        while (matchCount > 1)
        {
            Assert.True(match.BanMap(votePlayer, 0));
            Assert.False(match.BanMap(votePlayer, 0));
            if (matchCount > 2)
            {
                Assert.Equal(MatchState.MapVote, match.CurrentState);
            }

            votePlayer = votePlayer == player1 ? player2 : player1;
            matchCount--;
        }

        Assert.Equal(MatchState.TeamVote, match.CurrentState);
        return votePlayer;
    }

    private static void SetPlayersReady(Match match, IPlayer player1, IPlayer player2, MatchState expectedMatchStateAfterReady)
    {
        match.TogglePlayerIsReady(player1);
        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);
        match.TogglePlayerIsReady(player2);
        Assert.Equal(expectedMatchStateAfterReady, match.CurrentState);
    }

    private static void ConnectPlayers(List<IPlayer> matchPlayers, Match match, IPlayer player1, IPlayer player2)
    {
        matchPlayers.Add(player1);
        Assert.True(match.TryAddPlayer(player1));
        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        matchPlayers.Add(player2);
        Assert.True(match.TryAddPlayer(player2));
        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);
    }

    private static MatchConfig CreateExampleConfig(IEnumerable<string>? mapList = null)
    {
        IEnumerable<string> mapListInternal = new List<string> { "de_dust2", "de_overpass", "de_inferno" };

        if (mapList != null)
        {
            mapListInternal = mapList;
        }

        var matchConfig = new MatchConfig
        {
            MatchId = "1337",
            PlayersPerTeam = 1,
            MinPlayersToReady = 1,
            NumMaps = 1,
            Team1 = new Config.Team
            {
                Id = "1",
                Name = "Team1",
                Players = new Dictionary<ulong, string>()
                {
                    { 0,"Abc" },
                },
            },
            Team2 = new Config.Team
            {
                Id = "2",
                Name = "Team2",
                Players = new Dictionary<ulong, string>()
                {
                    { 1,"Def" },
                },
            },
        };

        foreach (var map in mapListInternal)
        {
            matchConfig.Maplist.Add(map);
        }

        return matchConfig;
    }

    private static IPlayer CreatePlayerSub(ulong steamId, int playerId)
    {
        var playerTeam = Contract.Team.None;
        var player = Substitute.For<IPlayer>();
        player.SteamID.Returns(steamId);
        player.UserId.Returns(playerId);
        player.Team.Returns(_ => playerTeam);
        player.When(player => player.SwitchTeam(Arg.Any<Contract.Team>())).Do(c => playerTeam = c.Arg<Contract.Team>());
        return player;
    }
}