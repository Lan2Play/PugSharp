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

            // Connect Players
            matchPlayers.Add(player1);
            Assert.True(match.TryAddPlayer(player1));
            Assert.Equal(MatchState.WaitingForPlayersConnected, match.CurrentState);

            matchPlayers.Add(player2);
            Assert.True(match.TryAddPlayer(player2));
            Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);

            // Set Ready for Players
            match.TogglePlayerIsReady(player1);
            Assert.Equal(MatchState.WaitingForPlayersConnectedReady, match.CurrentState);
            match.TogglePlayerIsReady(player2);
            Assert.Equal(MatchState.MapVote, match.CurrentState);

            // Vote Map
            var matchCount = config.Maplist.Count();
            var votePlayer = player1;

            Assert.False(match.BanMap(votePlayer, matchCount.ToString()));
            Assert.False(match.BanMap(votePlayer, "abc"));

            while (matchCount > 1)
            {
                Assert.True(match.BanMap(votePlayer, "0"));
                Assert.False(match.BanMap(votePlayer, "0"));
                if (matchCount > 2)
                {
                    Assert.Equal(MatchState.MapVote, match.CurrentState);
                }

                votePlayer = votePlayer == player1 ? player2 : player1;
                matchCount--;
            }

            Assert.Equal(MatchState.TeamVote, match.CurrentState);

            // Vote Team
            Assert.False(match.VoteTeam(votePlayer, "pizza"));
            Assert.False(match.VoteTeam(votePlayer == player1 ? player2 : player1, "T"));
            Assert.True(match.VoteTeam(votePlayer, "T"));

            Assert.Equal(MatchState.SwitchMap, match.CurrentState);
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