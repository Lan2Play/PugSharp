using PugSharp.Api.G5Api.Tests.Fixtures;

using Xunit;
using Xunit.Abstractions;

namespace PugSharp.Api.G5Api.Tests;

public partial class G5ApiTests : ApiTestBase, IClassFixture<G5ApiFixture>
{
    public G5ApiTests(G5ApiFixture api, ITestOutputHelper testOutput) : base(api, testOutput)
    {
    }

    [Fact/*(Skip = "Not working on server")*/]
    public async Task I_can_create_a_register_token()
    {
        // Arrange
        var g5ApiClient = await Api.CreateClientAsync();

        // Act
        Assert.True(await g5ApiClient.SendEventAsync(new MapVetoedEvent() { Team = "team1", MapName = "de_dust2", MatchId = "1" }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new MapVetoedEvent() { Team = "team2", MapName = "de_cache", MatchId = "1" }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new MapPickedEvent() { Team = "team2", MapName = "de_mirage", MatchId = "1", MapNumber = 1 }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new GoingLiveEvent() { MapNumber = 1, MatchId = "1" }, CancellationToken.None));

        Assert.True(await g5ApiClient.SendEventAsync(new RoundMvpEvent() { MapNumber = 1, MatchId = "1", Player = new Player("76561198025644200", "3", 3, Side.T, isBot: false), Reason = 2, RoundNumber = 1 }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new RoundMvpEvent() { MapNumber = 1, MatchId = "1", Player = new Player("76561198025644200", "3", 3, Side.T, isBot: false), Reason = 2, RoundNumber = 2 }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new RoundMvpEvent() { MapNumber = 1, MatchId = "1", Player = new Player("76561198025644200", "3", 3, Side.T, isBot: false), Reason = 2, RoundNumber = 3 }, CancellationToken.None));

        var statsTeam1 = new StatsTeam("1", "Kraddlers", 1, 0, 0, 0, new List<StatsPlayer>
        {
            new() { SteamId= "76561198025644200", Name = "3", Stats = new PlayerStats()},
            new() {SteamId= "76561198025644200", Name = "3", Stats =  new PlayerStats()},
        });
        var statsTeam2 = new StatsTeam("2", "Knaddles", 1, 3, 2, 1, new List<StatsPlayer>
        {
            new() {SteamId= "76561198025644200", Name = "3", Stats = new PlayerStats()},
            new() {SteamId= "76561198025644200", Name = "3", Stats = new PlayerStats()},
         });

        // Assert
        Assert.True(await g5ApiClient.SendEventAsync(new MapResultEvent() { MapNumber = 1, MatchId = "1", Winner = new Winner(Side.T, 0), StatsTeam1 = statsTeam1, StatsTeam2 = statsTeam2 }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new SeriesResultEvent() { MatchId = "1", Winner = new Winner(Side.T, 0), Team1SeriesScore = 0, Team2SeriesScore = 1, TimeUntilRestore = 2 }, CancellationToken.None));

        
    }
}
