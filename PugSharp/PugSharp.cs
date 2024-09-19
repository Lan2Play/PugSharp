using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;
using Polly.Extensions.Http;

using PugSharp.Api.Contract;
using PugSharp.Api.G5Api;
using PugSharp.Api.Json;
using PugSharp.ApiStats;
using PugSharp.Config;
using PugSharp.Match;
using PugSharp.Server.Contract;
using PugSharp.Translation;

namespace PugSharp;

[MinimumApiVersion(110)]
public class PugSharp : BasePlugin, IBasePlugin
{
    private const int _RetryCount = 3;
    private const int _RetryDelayFactor = 2;

    private ServiceProvider? _ServiceProvider;
    private IApplication? _Application;

    public override string ModuleName => "PugSharp Plugin";

    public override string ModuleVersion => "0.0.1";

    public PugSharp()
    {
        Console.WriteLine("Ctor PugSharp");
    }

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Start Loading PugSharp");
        base.Load(hotReload);

        // Create DI container
        var services = new ServiceCollection();

        services.AddLogging(options => options.AddConsole());

        var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ILoggerFactory));
        if (serviceDescriptor != null)
        {
            services.Remove(serviceDescriptor);
        }

        services.AddSingleton<ICssDispatcher, CssDispatcher>();
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

        services.AddApiStats(retryPolicy);

        services.AddSingleton<JsonApiProvider>();

        services.AddSingleton<ITextHelper>(sp => new TextHelper(sp.GetRequiredService<ILogger<TextHelper>>(), ChatColors.Blue, ChatColors.Green, ChatColors.Red));

        services.AddSingleton<G5CommandProvider>();

        // Build service provider
        _ServiceProvider = services.BuildServiceProvider();

        // Instantiate Application class where event handlers and other things will be declared
        _Application = _ServiceProvider.GetRequiredService<IApplication>();

        Console.WriteLine("Start initialize PugSharp Application");
        _Application.Initialize(hotReload);

        Console.WriteLine("PugSharp Loaded");
    }

    /// <summary>
    /// Method that is called on unload of the plugin
    /// </summary>
    /// <param name="hotReload">Is called from hot reload</param>
    public override void Unload(bool hotReload)
    {
        Console.WriteLine("Start Unloading PugSharp");
        // Remove reference
        _Application?.Dispose();
        _Application = null;

        // Dispose service provider
        _ServiceProvider?.Dispose();
        _ServiceProvider = null;

        base.Unload(hotReload);
        Console.WriteLine("PugSharp Unloaded");
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
