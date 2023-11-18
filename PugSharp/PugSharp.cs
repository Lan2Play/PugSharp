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
using PugSharp.Match;

namespace PugSharp;

public class PugSharp : BasePlugin, IBasePlugin
{
    private ServiceProvider? _ServiceProvider;
    private IApplication? _Application;

    public override string ModuleName => "PugSharp Plugin";

    public override string ModuleVersion => "0.0.1";

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        //var config = new LoggingConfiguration();
        //config.AddTarget(new FileTarget
        //{
        //    Name = "PugSharpoLogger",
        //    FileName = Path.Combine(CounterStrikeSharp.API.Server.GameDirectory, "csgo", "PugSharp", "Logs", nameof(PugSharp) + ".log"),
        //});
        //config.AddTarget(new ColoredConsoleTarget { Name = "PugSharpConsoleLogger", });

        // Create DI container
        var services = new ServiceCollection();

        services.AddLogging(options =>
        {
            options.AddConsole();
            //options.ClearProviders();
            //options.AddNLog(config);
        });

        services.AddSingleton<ICsServer, CsServer>();
        services.AddSingleton<MultiApiProvider>();
        services.AddSingleton<IApiProvider>(s => s.GetRequiredService<MultiApiProvider>());

        services.AddSingleton<IBasePlugin>(this);

        services.AddSingleton<IApplication, Application>();

        services.AddSingleton<ConfigProvider>();

        services.AddTransient<MatchFactory>();

        // TODO Add HttpClients for ApiProviders
        services.AddSingleton<G5ApiProvider>();
        services.AddSingleton<ApiStats.ApiStats>();
        services.AddSingleton<JsonApiProvider>();
        services.AddSingleton<ITextHelper>(sp => new TextHelper(sp.GetRequiredService<ILogger<TextHelper>>(), ChatColors.Blue, ChatColors.Green, ChatColors.Red));
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
        _Application?.Dispose();
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
            // Remove reference
            _Application = null;

            // Dispose service provider
            _ServiceProvider?.Dispose();
            _ServiceProvider = null;
        }
    }
}
