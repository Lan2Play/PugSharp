using PugSharp.Api.Contract;
using PugSharp.Api.G5Api;
using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using PugSharp.Server.Contract;
using System.Text.RegularExpressions;

namespace PugSharp;

public partial class G5ApiProvider : IApiProvider
{
    private static readonly ILogger<G5ApiProvider> _Logger = LogManager.CreateLogger<G5ApiProvider>();
    private readonly G5ApiClient _G5Stats;
    private readonly ICsServer _CsServer;

    public G5ApiProvider(G5ApiClient apiClient, ICsServer csServer)
    {
        _G5Stats = apiClient;
        _CsServer = csServer;
    }

    #region IApiProvider

    public Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken)
    {
        var (CtScore, TScore) = _CsServer.LoadTeamsScore();
        var emptyStatsTeam = new StatsTeam(string.Empty, string.Empty, 0, 0, 0, 0, Enumerable.Empty<StatsPlayer>());
        return _G5Stats.SendEventAsync(new MapResultEvent(finalizeMapParams.MatchId, finalizeMapParams.MapNumber, new Winner(CtScore > TScore ? Side.CT : Side.T, finalizeMapParams.Team1Score > finalizeMapParams.Team2Score ? 1 : 2), emptyStatsTeam, emptyStatsTeam), cancellationToken);
    }

    public Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken)
    {
        return _G5Stats.SendEventAsync(new GoingLiveEvent(goingLiveParams.MatchId, goingLiveParams.MapNumber), cancellationToken);
    }

    public Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken)
    {
        var (CtScore, TScore) = _CsServer.LoadTeamsScore();
        return _G5Stats.SendEventAsync(new SeriesResultEvent(seriesResultParams.MatchId, new Winner(CtScore > TScore ? Side.CT : Side.T, seriesResultParams.Team1SeriesScore > seriesResultParams.Team2SeriesScore ? 1 : 2), seriesResultParams.Team1SeriesScore, seriesResultParams.Team2SeriesScore, (int)seriesResultParams.TimeBeforeFreeingServerMs), cancellationToken);
    }

    public Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Get5Api Commands


    #endregion
}

public partial class G5CommandProvider : ICommandProvider
{
    private static readonly ILogger<G5ApiProvider> _Logger = LogManager.CreateLogger<G5ApiProvider>();
    private readonly ICsServer _CsServer;

    public G5CommandProvider(ICsServer csServer)
    {
        _CsServer = csServer;
    }

    public IReadOnlyList<ProviderCommand> LoadProviderCommands()
    {
        return new List<ProviderCommand>()
        {
            new("version","Return the cs server version", CommandVersion),
            new("get5_status","Return the get 5 status", CommandGet5Status),
            new("get5_loadmatch_url","Load a match with the given URL and API key for a match", CommandLoadMatchUrl),
            new("get5_endmatch","Ends the match", CommandEndMatch),
            new("sm_pause","Pauses the match", CommandSmPause),
            new("sm_unpause","Unpauses the match", CommandSmUnpause),
            new("get5_addplayer","Adds a player", CommandAddPlayer),
            new("get5_addcoach","Adds a coach", CommandAddCoach),
            new("get5_removeplayer","Removes a player", CommandRemovePlayer),
            new("get5_listbackups","List all Backups", CommandListBackUps),
            new("get5_loadbackup","Load a backup", CommandLoadBackUp),
            new("get5_loadbackup_url","Loac a backup", CommandLoadBackUpUrl),
        };
    }

    private IEnumerable<string> CommandLoadBackUpUrl(string[] arg)
    {
        return new[] { "Not yet supported!" };
    }

    private IEnumerable<string> CommandLoadBackUp(string[] arg)
    {
        return new[] { "Not yet supported!" };
    }

    private IEnumerable<string> CommandListBackUps(string[] arg)
    {
        return new[] { "Not yet supported!" };
    }

    private IEnumerable<string> CommandRemovePlayer(string[] arg)
    {
        return new[] { "Not yet supported!" };
    }

    private IEnumerable<string> CommandAddCoach(string[] arg)
    {
        return new[] { "Not yet supported!" };
    }

    private IEnumerable<string> CommandAddPlayer(string[] arg)
    {
        return new[] { "Not yet supported!" };
    }

    private IEnumerable<string> CommandSmUnpause(string[] arg)
    {
        _CsServer.ExecuteCommand("css_unpause");
        return Enumerable.Empty<string>();
    }

    private IEnumerable<string> CommandSmPause(string[] arg)
    {
        _CsServer.ExecuteCommand("css_pause");
        return Enumerable.Empty<string>();
    }

    private IEnumerable<string> CommandEndMatch(string[] arg)
    {
        _CsServer.ExecuteCommand("ps_stopmatch");
        return Enumerable.Empty<string>();
    }

    private IEnumerable<string> CommandLoadMatchUrl(string[] args)
    {
        _CsServer.ExecuteCommand($"ps_loadconfig {string.Join(' ', args.Skip(1).Where(x => !x.Contains("Authorization", StringComparison.OrdinalIgnoreCase)).Select(x => $"\"{x}\""))}");
        return Enumerable.Empty<string>();
    }

    private IEnumerable<string> CommandGet5Status(string[] args)
    {
        return new[] { "0.15.0" };
    }

    [GeneratedRegex(@"PatchVersion=(?<version>[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex PatchVersion();

    private IEnumerable<string> CommandVersion(string[] args)
    {
        string steamInfPath = Path.Combine(_CsServer.GameDirectory, "csgo", "steam.inf");

        if (File.Exists(steamInfPath))
        {
            try
            {
                var match = PatchVersion().Match(File.ReadAllText(steamInfPath));

                if (match.Success)
                {
                    return new[] { match.Groups["version"].Value };
                }

                _Logger.LogError("The 'PatchVersion' key could not be located in the steam.inf file.");
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "An error occurred while reading the 'steam.inf' file.");
            }
        }
        else
        {
            _Logger.LogError("The 'steam.inf' file was not found in the root directory of Counter-Strike 2. Path: \"{steamInfPath}\"", steamInfPath);
        }

        return Enumerable.Empty<string>();
    }
}