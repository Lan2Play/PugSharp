using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PugSharp.Api.Contract;
using PugSharp.Config;
using PugSharp.Server.Contract;
using PugSharp.Translation;

namespace PugSharp.Match;
public class MatchFactory
{
    private readonly IServiceProvider _ServiceProvider;

    public MatchFactory(IServiceProvider serviceProvider)
    {
        _ServiceProvider = serviceProvider;
    }

    public Match CreateMatch(MatchInfo matchInfo, string roundBackupFile)
    {
        return new Match(_ServiceProvider,
            _ServiceProvider.GetRequiredService<ILogger<Match>>(),
            _ServiceProvider.GetRequiredService<IApiProvider>(),
            _ServiceProvider.GetRequiredService<ITextHelper>(),
             _ServiceProvider.GetRequiredService<ICsServer>(),
            matchInfo,
            roundBackupFile);
    }

    public Match CreateMatch(MatchConfig matchConfig)
    {
        return new Match(_ServiceProvider,
            _ServiceProvider.GetRequiredService<ILogger<Match>>(),
            _ServiceProvider.GetRequiredService<IApiProvider>(),
            _ServiceProvider.GetRequiredService<ITextHelper>(),
             _ServiceProvider.GetRequiredService<ICsServer>(),
            matchConfig);
    }
}
