using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using PugSharp.Api.Contract;
using PugSharp.Config;
using PugSharp.Match;
using PugSharp.Server.Contract;
using PugSharp.Translation;

namespace PugSharp.Tests;

public class ApplicationTests
{
    private static ServiceProvider CreateTestProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<ICsServer>());
        services.AddSingleton<IApiProvider, MultiApiProvider>();

        services.AddSingleton(Substitute.For<ICssDispatcher>());
        services.AddSingleton(Substitute.For<ICsServer>());
        services.AddLogging();
        services.AddSingleton<IApplication, Application>();

        services.AddSingleton<ConfigProvider>();
        services.AddTransient<MatchFactory>();
        services.AddSingleton<MultiApiProvider>();
        services.AddSingleton<G5CommandProvider>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton(Substitute.For<ITextHelper>());
        services.AddSingleton(Substitute.For<IBasePlugin>());

        // Build service provider
        return services.BuildServiceProvider();
    }

    [Fact]
    public void InitializeApplicationTest()
    {
        var serviceProvider = CreateTestProvider();
        var application = serviceProvider.GetRequiredService<IApplication>();
        Assert.NotNull(application);

        application.Initialize(hotReload: false);
    }
}