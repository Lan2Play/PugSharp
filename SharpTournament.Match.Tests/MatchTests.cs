using NSubstitute;
using SharpTournament.Config;
using SharpTournament.Match.Contract;

namespace SharpTournament.Match.Tests
{
    public class MatchTests
    {
        [Fact]
        public void WrongPlayerConnectTest()
        {
            var matchCallback = Substitute.For<IMatchCallback>();
            MatchConfig config = CreateExampleConfig();

            var match = new Match(matchCallback, config);

            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            IPlayer player = CreatePlayerSub(1337, 0);

            Assert.False(match.TryAddPlayer(player));
        }

        [Fact]
        public void CorrectPlayerConnectTest()
        {
            var matchCallback = Substitute.For<IMatchCallback>();
            MatchConfig config = CreateExampleConfig();

            var match = new Match(matchCallback, config);

            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            IPlayer player = CreatePlayerSub(0, 0);

            Assert.True(match.TryAddPlayer(player));
        }

        [Fact]
        public void MatchTest()
        {
            var matchPlayers = new List<IPlayer>();
            var matchCallback = Substitute.For<IMatchCallback>();
            matchCallback.GetAllPlayers().Returns(matchPlayers);

            MatchConfig config = CreateExampleConfig();

            var match = new Match(matchCallback, config);

            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            IPlayer player1 = CreatePlayerSub(0, 0);
            IPlayer player2 = CreatePlayerSub(1, 1);

            matchPlayers.Add(player1);
            Assert.True(match.TryAddPlayer(player1));
            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            matchPlayers.Add(player2);
            Assert.True(match.TryAddPlayer(player2));
            Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

            match.TogglePlayerIsReady(player1);
            Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);
            match.TogglePlayerIsReady(player2);
            Assert.Equal(MatchState.MapVote, match.CurrentState);

            var matchCount = config.Maplist.Count();
            var vetoPlayer = player1;

            Assert.False(match.SetVeto(vetoPlayer, matchCount.ToString()));
            Assert.False(match.SetVeto(vetoPlayer, "abc"));

            while (matchCount > 1)
            {
                Assert.True(match.SetVeto(vetoPlayer, "0"));
                Assert.False(match.SetVeto(vetoPlayer, "0"));
                if (matchCount > 2)
                {
                    Assert.Equal(MatchState.MapVote, match.CurrentState);
                }

                vetoPlayer = vetoPlayer == player1 ? player2 : player1;
                matchCount--;
            }

            Assert.Equal(MatchState.TeamVote, match.CurrentState);
        }

        private static MatchConfig CreateExampleConfig()
        {
            return new MatchConfig
            {
                PlayersPerTeam = 1,
                MinPlayersToReady = 1,
                Team1 = new Config.Team
                {
                    Name = "Team1",
                    Players = new()
                    {
                        { 0,"Abc" },
                    },
                },
                Team2 = new Config.Team
                {
                    Name = "Team2",
                    Players = new()
                    {
                        { 1,"Def" },
                    },
                },
                Maplist = new string[] { "de_dust2", "de_overpass", "de_inferno" }
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
}