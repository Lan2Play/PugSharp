using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using PugSharp.Api.Contract;
using PugSharp.Config;
using PugSharp.Server.Contract;
using Microsoft.Extensions.DependencyInjection;
using PugSharp.Api.Json;
using PugSharp.Translation;
using CounterStrikeSharp.API.Modules.Utils;
using PugSharp.ApiStats;

namespace PugSharp;

public class PugSharp : BasePlugin, IBasePlugin
{
    private readonly CancellationTokenSource _CancellationTokenSource = new();

    private ServiceProvider? _ServiceProvider;
    private IApplication? _Application;

    public override string ModuleName => "PugSharp Plugin";

    public override string ModuleVersion => "0.0.1";

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        // Create DI container
        var services = new ServiceCollection();

        services.AddLogging(options =>
        {
            options.AddConsole();
            //options.AddSimpleConsole(o => o.SingleLine = true);
        });

        services.AddSingleton<ICsServer, CsServer>();
        services.AddSingleton<IApiProvider, MultiApiProvider>();

        services.AddSingleton<IBasePlugin>(this);

        services.AddSingleton<IApplication, Application>();

        services.AddSingleton<ConfigProvider>();

        services.AddTransient<Match.Match>();

        // TODO Add HttpClients for ApiProviders
        services.AddSingleton<G5ApiProvider>();
        services.AddSingleton<ApiStats.ApiStats>();
        services.AddSingleton<JsonApiProvider>();
        services.AddSingleton<ITextHelper>(services => new TextHelper(services.GetRequiredService<ILogger<TextHelper>>(), ChatColors.Blue, ChatColors.Green, ChatColors.Red));
        services.AddSingleton<DemoUploader>();
        services.AddSingleton<G5CommandProvider>();

        // Build service provider
        _ServiceProvider = services.BuildServiceProvider();

        // Instantiate Application class where event handlers and other things will be declared
        _Application = _ServiceProvider.GetRequiredService<IApplication>();
        _Application.Initialize(hotReload);
    }

    /// <summary>
    /// Method that is called on unload of the plugin
    /// </summary>
    /// <param name="hotReload">Is called from hot reload</param>
    public override void Unload(bool hotReload)
    {
        // Remove reference
        _Application = null;

        // Dispose service provider
        _ServiceProvider?.Dispose();
        _ServiceProvider = null;

        base.Unload(hotReload);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _CancellationTokenSource.Cancel();
            _CancellationTokenSource.Dispose();
        }
    }
}
