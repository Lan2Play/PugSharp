using PugSharp.Api.G5Api.Tests.Fixtures;

using Xunit;
using Xunit.Abstractions;

namespace PugSharp.Api.G5Api.Tests;

public partial class G5ApiTests : ApiTestBase
{
    public G5ApiTests(G5ApiFixture api, ITestOutputHelper testOutput) : base(api, testOutput)
    {
    }

    [Fact(Skip = "Not working on server")]
    public async Task I_can_create_a_register_token()
    {
        // Arrange
        var g5ApiClient = await Api.CreateClientAsync();



        // Act
        Assert.True(await g5ApiClient.SendEventAsync(new MapVetoedEvent() { Team = "team1", MapName = "de_dust2", MatchId = "1" }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new MapVetoedEvent() { Team = "team2", MapName = "de_cache", MatchId = "1" }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new MapPickedEvent() { Team = "team2", MapName = "de_mirage", MatchId = "1", MapNumber = 1 }, CancellationToken.None));
        Assert.True(await g5ApiClient.SendEventAsync(new GoingLiveEvent() { MapNumber = 1, MatchId = "1" }, CancellationToken.None));

        // Assert
    }
}
