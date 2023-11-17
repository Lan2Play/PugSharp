using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using PugSharp.Api.Contract;
using PugSharp.Config;
using PugSharp.Logging;
using PugSharp.Server.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace PugSharp;

public class PugSharp : BasePlugin, IBasePlugin
{
    private static readonly ILogger<PugSharp> _Logger = LogManager.CreateLogger<PugSharp>();

    private readonly ICsServer _CsServer;
    private readonly ConfigProvider _ConfigProvider;

    private readonly CancellationTokenSource _CancellationTokenSource = new();

    private ServiceProvider _ServiceProvider;
    private IApplication _Application;

    public override string ModuleName => "PugSharp Plugin";

    public override string ModuleVersion => "0.0.1";

    public string PugSharpDirectory { get; }

    public PugSharp()
    {
        _CsServer = new CsServer();
        PugSharpDirectory = Path.Combine(_CsServer.GameDirectory, "csgo", "PugSharp");
        _ConfigProvider = new(Path.Join(PugSharpDirectory, "Config"));
    }

    public override void Load(bool hotReload)
    {
        _Logger.LogInformation("Loading PugSharp!");
        base.Load(hotReload);

        //if (!Config.IsEnabled)
        //{
        //    return;
        //}

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

        //services.AddSingleton(Config);
        services.AddSingleton<IApplication, Application>();

        services.AddSingleton<ConfigProvider>();

        services.AddTransient<Match.Match>();

        // Register other services here
        //services.AddSingleton<IPluginService, PluginService>();

        // Register facades here (Services that have an httpclient)
        services.AddHttpClient<IApiProvider, G5ApiProvider>("G5", (s, h) => { });
        services.AddHttpClient<IApiProvider, ApiStats.ApiStats>("ApiStats", (s, h) => { });

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

            _ConfigProvider.Dispose();
        }
    }
}
