using PugSharp.Api.G5Api.Tests.Fixtures;

using Xunit;
using Xunit.Abstractions;

namespace PugSharp.Api.G5Api.Tests;

public partial class G5ApiTests : ApiTestBase
{
    public G5ApiTests(G5ApiFixture api, ITestOutputHelper testOutput) : base(api, testOutput)
    {
    }

    [Fact]
    public async Task I_can_create_a_register_token()
    {
        // Arrange
        var g5ApiClient = await Api.CreateClientAsync();



        // Act
        await g5ApiClient.SendEventAsync(new MapVetoedEvent() { TeamNumber = 1, MapName = "de_dust2", MatchId = "1" }, CancellationToken.None);
        await g5ApiClient.SendEventAsync(new MapVetoedEvent() { TeamNumber = 2, MapName = "de_cache", MatchId = "1" }, CancellationToken.None);
        await g5ApiClient.SendEventAsync(new MapPickedEvent() { TeamNumber = 2, MapName = "de_mirage", MatchId = "1", MapNumber = 1 }, CancellationToken.None);
        await g5ApiClient.SendEventAsync(new GoingLiveEvent() { MapNumber = 1, MatchId = "1" }, CancellationToken.None);

        // Assert
    }
}
