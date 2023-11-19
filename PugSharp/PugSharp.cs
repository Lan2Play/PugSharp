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
using Polly.Extensions.Http;
using Polly;
using PugSharp.Api.G5Api;

namespace PugSharp;

public class PugSharp : BasePlugin, IBasePlugin
{
    private const int _RetryCount = 3;
    private const int _RetryDelayFactor = 2;

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
        });

        services.AddSingleton<ICsServer, CsServer>();
        services.AddSingleton<MultiApiProvider>();
        services.AddSingleton<IApiProvider>(s => s.GetRequiredService<MultiApiProvider>());

        services.AddSingleton<IBasePlugin>(this);

        services.AddSingleton<IApplication, Application>();

        var retryPolicy = HttpPolicyExtensions
         .HandleTransientHttpError()
             .WaitAndRetryAsync(_RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(_RetryDelayFactor, retryAttempt)));

        services.AddHttpClient<ConfigProvider>()
                .AddPolicyHandler(retryPolicy);

        services.AddSingleton<ConfigProvider>();

        services.AddTransient<MatchFactory>();

        services.AddHttpClient<G5ApiClient>()
                .AddPolicyHandler(retryPolicy);
        services.AddSingleton<G5ApiClient>();
        services.AddSingleton<G5ApiProvider>();

        services.AddHttpClient<ApiStats.ApiStats>()
               .AddPolicyHandler(retryPolicy);
        services.AddSingleton<ApiStats.ApiStats>();

        services.AddSingleton<JsonApiProvider>();

        services.AddSingleton<ITextHelper>(sp => new TextHelper(sp.GetRequiredService<ILogger<TextHelper>>(), ChatColors.Blue, ChatColors.Green, ChatColors.Red));

        services.AddHttpClient<DemoUploader>()
               .AddPolicyHandler(retryPolicy);
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
