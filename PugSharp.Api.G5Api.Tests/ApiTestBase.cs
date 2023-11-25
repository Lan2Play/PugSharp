using Xunit.Abstractions;
using Xunit;
using PugSharp.Api.G5Api.Tests.Fixtures;

namespace PugSharp.Api.G5Api.Tests;

[Collection(nameof(G5ApiFixtureCollection))]
public abstract class ApiTestBase : IDisposable
{
    protected G5ApiFixture Api { get; }

    protected ITestOutputHelper TestOutput { get; }

    protected ApiTestBase(G5ApiFixture api, ITestOutputHelper testOutput)
    {
        Api = api;
        TestOutput = testOutput;
    }

    public void Dispose()
    {
        // Ideally we should route the logs in realtime, but it's a bit tedious
        // with the way the TestContainers library is designed.
        TestOutput.WriteLine(Api.GetLogs());
    }
}
