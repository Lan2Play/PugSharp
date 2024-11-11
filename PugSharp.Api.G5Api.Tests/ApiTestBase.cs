using PugSharp.Api.G5Api.Tests.Fixtures;

using Xunit;
using Xunit.Abstractions;

namespace PugSharp.Api.G5Api.Tests;

[Collection(nameof(G5ApiFixtureCollection))]
public abstract class ApiTestBase : IDisposable
{
    private bool _DisposedValue;

    protected G5ApiFixture Api { get; }

    protected ITestOutputHelper TestOutput { get; }

    protected ApiTestBase(G5ApiFixture api, ITestOutputHelper testOutput)
    {
        Api = api;
        TestOutput = testOutput;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_DisposedValue)
        {
            if (disposing)
            {
                TestOutput.WriteLine(Api.GetLogs());
            }

            _DisposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
