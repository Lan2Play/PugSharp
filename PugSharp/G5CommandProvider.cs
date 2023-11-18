using PugSharp.Api.Contract;
using Microsoft.Extensions.Logging;
using PugSharp.Server.Contract;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PugSharp;

public sealed partial class G5CommandProvider : ICommandProvider
{
    private const int _RegexTimeout = 1000;
    private readonly ILogger<G5ApiProvider> _Logger;
    private readonly ICsServer _CsServer;

    public G5CommandProvider(ICsServer csServer, ILogger<G5ApiProvider> logger)
    {
        _Logger = logger;
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
        // TODO reply from G5Api is different than the callback from eventula! All the cvars are ignored
        _CsServer.ExecuteCommand($"ps_loadconfig {string.Join(' ', args.Skip(1).Where(x => !x.Contains("Authorization", StringComparison.OrdinalIgnoreCase)).Select(x => $"\"{x}\""))}");
        return Enumerable.Empty<string>();
    }

    internal class Get5Status
    {
        [JsonPropertyName("plugin_version")]
        public required string PluginVersion { get; set; }

        [JsonPropertyName("gamestate")]
        public int GameState { get; set; }
    }

    private IEnumerable<string> CommandGet5Status(string[] args)
    {
        return new[] { JsonSerializer.Serialize(new Get5Status { PluginVersion = "0.15.0", GameState = 0 }) };
    }

    [GeneratedRegex(@"PatchVersion=(?<version>[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)", RegexOptions.ExplicitCapture, _RegexTimeout)]
    private static partial Regex PatchVersionRegex();

    [GeneratedRegex(@"ClientVersion=(?<version>[0-9]+)", RegexOptions.ExplicitCapture, _RegexTimeout)]
    private static partial Regex ClientVersionRegex();

    [GeneratedRegex(@"ServerVersion=(?<version>[0-9]+)", RegexOptions.ExplicitCapture, _RegexTimeout)]
    private static partial Regex ServerVersionRegex();

    [GeneratedRegex(@"ProductName=(?<productname>.+)", RegexOptions.ExplicitCapture, _RegexTimeout)]
    private static partial Regex ProductNameRegex();

    private static string LoadSteamInfValue(string steamInf, Regex regex, string regexGroupName, List<string> errors)
    {
        var result = regex.Match(steamInf);
        if (!result.Success)
        {
            errors.Add("Error loading patchVersion");
            return string.Empty;
        }

        return result.Groups[regexGroupName].Value;
    }

    private IEnumerable<string> CommandVersion(string[] args)
    {
        string steamInfPath = Path.Combine(_CsServer.GameDirectory, "csgo", "steam.inf");

        if (File.Exists(steamInfPath))
        {
            try
            {
                var errors = new List<string>();
                var steamInf = File.ReadAllText(steamInfPath);

                var patchVersion = LoadSteamInfValue(steamInf, PatchVersionRegex(), "version", errors);
                var clientVersion = LoadSteamInfValue(steamInf, ClientVersionRegex(), "version", errors);
                var serverVersion = LoadSteamInfValue(steamInf, ServerVersionRegex(), "version", errors);
                var productName = LoadSteamInfValue(steamInf, ProductNameRegex(), "productname", errors);

                if (errors.Any())
                {
                    return errors;
                }

                return new[] {
                        $"Protocol version {patchVersion.Replace(".","", StringComparison.OrdinalIgnoreCase)} [{clientVersion}/{serverVersion}]",
                        $"Exe version {patchVersion} ({productName})",
                    };

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