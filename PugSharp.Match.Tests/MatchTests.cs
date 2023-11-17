using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PugSharp.Api.Contract;
using PugSharp.Api.Json;
using PugSharp.ApiStats;
using PugSharp.Config;
using PugSharp.Match.Contract;
using PugSharp.Server.Contract;
using PugSharp.Translation;

namespace PugSharp.Match.Tests;

public class MatchTests
{
    private static IServiceProvider CreateTestProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<ICsServer>());
        services.AddSingleton<IApiProvider, MultiApiProvider>();

        services.AddSingleton(Substitute.For<ICsServer>());
        services.AddLogging(options =>
        {
            options.AddConsole();
        });
        services.AddSingleton<IApplication, Application>();

        services.AddSingleton<ConfigProvider>();
        services.AddTransient<Match>();

        services.AddSingleton<G5ApiProvider>();
        services.AddSingleton<ApiStats.ApiStats>();
        services.AddSingleton<JsonApiProvider>();
        services.AddSingleton(Substitute.For<ITextHelper>());
        services.AddSingleton<DemoUploader>();

        // Build service provider
        return services.BuildServiceProvider();
    }

    [Fact]
    public void CreateDotGraphTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();
        var match = serviceProvider.GetRequiredService<Match>();
        match.Initialize(config);

        var dotGraphString = match.CreateDotGraph();
        Assert.True(!string.IsNullOrEmpty(dotGraphString));
    }

    [Fact]
    public void WrongPlayerConnectTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();
        var match = serviceProvider.GetRequiredService<Match>();
        match.Initialize(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player = CreatePlayerSub(1337, 0);

        Assert.False(match.TryAddPlayer(player));
    }

    [Fact]
    public void CorrectPlayerConnectTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();
        var match = serviceProvider.GetRequiredService<Match>();
        match.Initialize(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player = CreatePlayerSub(0, 0);

        Assert.True(match.TryAddPlayer(player));
    }

    [Fact]
    public async Task MatchTest()
    {
        MatchConfig config = CreateExampleConfig();

        var serviceProvider = CreateTestProvider();

        var matchPlayers = new List<IPlayer>();
        var csServer = serviceProvider.GetRequiredService<ICsServer>();
        csServer.LoadAllPlayers().Returns(matchPlayers);

        var match = serviceProvider.GetRequiredService<Match>();
        match.Initialize(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player1 = CreatePlayerSub(0, 0);
        IPlayer player2 = CreatePlayerSub(1, 1);

        ConnectPlayers(matchPlayers, match, player1, player2);
        await SetPlayersReady(match, player1, player2, MatchState.MapVote);
        IPlayer votePlayer = VoteForMap(config, match, player1, player2);
        await VoteTeam(csServer, config, match, player1, player2, votePlayer);
        PauseUnpauseMatch(csServer, match, player1);
    }

    [Fact]
    public async Task MatchTestWithOneMap()
    {
        MatchConfig config = CreateExampleConfig(new List<string> { "de_dust2" });
        var serviceProvider = CreateTestProvider();

        var matchPlayers = new List<IPlayer>();
        var csServer = serviceProvider.GetRequiredService<ICsServer>();
        csServer.LoadAllPlayers().Returns(matchPlayers);

        var match = serviceProvider.GetRequiredService<Match>();
        match.Initialize(config);

        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

        IPlayer player1 = CreatePlayerSub(0, 0);
        IPlayer player2 = CreatePlayerSub(1, 1);

        ConnectPlayers(matchPlayers, match, player1, player2);
        await SetPlayersReady(match, player1, player2, MatchState.TeamVote);
        await VoteTeam(csServer, config, match, player1, player2, player1);
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

    private static async Task VoteTeam(ICsServer csServer, MatchConfig config, Match match, IPlayer player1, IPlayer player2, IPlayer votePlayer)
    {
        Assert.False(match.VoteTeam(votePlayer, "pizza"));
        Assert.False(match.VoteTeam(votePlayer == player1 ? player2 : player1, "T"));
        Assert.True(match.VoteTeam(votePlayer, "T"));

        csServer.Received().SwitchMap(config.Maplist[^1]);

        Assert.Equal(MatchState.WaitingForPlayersReady, match.CurrentState);
        await match.TogglePlayerIsReadyAsync(player1).ConfigureAwait(false);
        Assert.Equal(MatchState.WaitingForPlayersReady, match.CurrentState);
        await match.TogglePlayerIsReadyAsync(player2).ConfigureAwait(false);

        Assert.Equal(MatchState.MatchRunning, match.CurrentState);
    }

    private static IPlayer VoteForMap(MatchConfig config, Match match, IPlayer player1, IPlayer player2)
    {
        var matchCount = config.Maplist.Length;
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

    private static async Task SetPlayersReady(Match match, IPlayer player1, IPlayer player2, MatchState expectedMatchStateAfterReady)
    {
        await match.TogglePlayerIsReadyAsync(player1).ConfigureAwait(false);
        Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);
        await match.TogglePlayerIsReadyAsync(player2).ConfigureAwait(false);
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

        return new MatchConfig
        {
            MatchId = "1337",
            PlayersPerTeam = 1,
            MinPlayersToReady = 1,
            NumMaps = 1,
            Team1 = new Config.Team
            {
                Name = "Team1",
                Players = new Dictionary<ulong, string>()
                {
                    { 0,"Abc" },
                },
            },
            Team2 = new Config.Team
            {
                Name = "Team2",
                Players = new Dictionary<ulong, string>()
                {
                    { 1,"Def" },
                },
            },
            Maplist = mapListInternal.ToArray(),
        };
    }

    private static IPlayer CreatePlayerSub(ulong steamId, int playerId)
    {
        var player = Substitute.For<IPlayer>();
        player.SteamID.Returns(steamId);
        player.UserId.Returns(playerId);
        return player;
    }
}