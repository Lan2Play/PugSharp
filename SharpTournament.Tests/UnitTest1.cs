using NSubstitute;
using SharpTournament.Config;
using SharpTournament.Match.Contract;

namespace SharpTournament.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var sharpTournament = new SharpTournament();
            var testUrlConfig = "https://dev.lan2play.de/api/matchmaking/40/configure/1";
            var authToken = "62|jrHJtxIWYE2r9Uh6ippvPv3wqlDhfDBvQyIUo624";
            Assert.True(sharpTournament.LoadConfig(testUrlConfig, authToken));
        }

        [Fact]
        public void WrongPlayerConnectTest()
        {
            var matchCallback = Substitute.For<IMatchCallback>();
            MatchConfig config = CreateExampleConfig();

            var match = new Match.Match(matchCallback, config);

            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            IPlayer player = CreatePlayerSub(1337);

            Assert.False(match.TryAddPlayer(player));
        }

        [Fact]
        public void CorrectPlayerConnectTest()
        {
            var matchCallback = Substitute.For<IMatchCallback>();
            MatchConfig config = CreateExampleConfig();

            var match = new Match.Match(matchCallback, config);

            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            IPlayer player = CreatePlayerSub(0);

            Assert.True(match.TryAddPlayer(player));
        }

        [Fact]
        public void MatchTest()
        {
            var matchPlayers = new List<IPlayer>();
            var matchCallback = Substitute.For<IMatchCallback>();
            matchCallback.GetAllPlayers().Returns(matchPlayers);

            MatchConfig config = CreateExampleConfig();

            var match = new Match.Match(matchCallback, config);

            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            IPlayer player1 = CreatePlayerSub(0);
            IPlayer player2 = CreatePlayerSub(1);

            matchPlayers.Add(player1);
            Assert.True(match.TryAddPlayer(player1));
            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            matchPlayers.Add(player2);
            Assert.True(match.TryAddPlayer(player2));
            Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);
        }

        private static MatchConfig CreateExampleConfig()
        {
            return new MatchConfig
            {
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
                }
            };
        }

        private static IPlayer CreatePlayerSub(ulong steamId)
        {
            var player = Substitute.For<IPlayer>();
            player.SteamID.Returns(steamId);
            return player;
        }
    }
}