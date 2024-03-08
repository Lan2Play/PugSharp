using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using OneOf;
using OneOf.Types;

namespace PugSharp.Config;

public class ConfigProvider
{
    private readonly HttpClient _HttpClient;
    private readonly ILogger<ConfigProvider> _Logger;

    private string _ConfigDirectory = string.Empty;
    private ServerConfig? _ServerConfig;

    public ConfigProvider(HttpClient httpClient, ILogger<ConfigProvider> logger)
    {
        _HttpClient = httpClient;
        _Logger = logger;
    }

    public void Initialize(string configDirectory)
    {
        _ConfigDirectory = configDirectory;
    }

    public async Task<OneOf<Error<string>, MatchConfig>> LoadMatchConfigFromFileAsync(string fileName)
    {
        var fullFileName = Path.IsPathRooted(fileName) ? fileName : Path.Combine(_ConfigDirectory, fileName);
        _Logger.LogInformation("Loading match from \"{fileName}\"", fullFileName);

        try
        {
            var configFileStream = File.OpenRead(fullFileName);
            await using (configFileStream.ConfigureAwait(false))
            {
                var config = await JsonSerializer.DeserializeAsync<MatchConfig>(configFileStream).ConfigureAwait(false);

                if (config == null)
                {
                    _Logger.LogError("MatchConfig was deserialized to null");

                    return new Error<string>("Config couldn't be deserialized");
                }

                _Logger.LogInformation("Successfully loaded config for match {matchId}", config.MatchId);
                return config;
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Failed loading config from {fileName}.", fullFileName);

            return new Error<string>($"Failed loading config from {fullFileName}.");
        }
    }

    public async Task<OneOf<Error<string>, MatchConfig>> LoadMatchConfigFromUrlAsync(string url, string authToken)
    {
        _Logger.LogInformation("Loading match from \"{url}\"", url);

        try
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            if (!string.IsNullOrEmpty(authToken))
            {
                httpRequestMessage.Headers.Add(nameof(HttpRequestHeader.Authorization), authToken);
            }

            var response = await _HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            try
            {
                var configJsonStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                var config = await JsonSerializer.DeserializeAsync<MatchConfig>(configJsonStream).ConfigureAwait(false);
                if (config == null)
                {
                    _Logger.LogError("MatchConfig was deserialized to null");

                    return new Error<string>("Config couldn't be deserialized");
                }

                configJsonStream.Seek(0, SeekOrigin.Begin);
                var matchConfigPath = Path.Combine(_ConfigDirectory, "match.json");
                var fileWriteStream = File.Open(matchConfigPath, FileMode.Create);
                await using (fileWriteStream.ConfigureAwait(false))
                {
                    await configJsonStream.CopyToAsync(fileWriteStream).ConfigureAwait(false);
                    configJsonStream.Close();
                }

                _Logger.LogInformation("Successfully loaded config for match {matchId}", config.MatchId);
                return config;
            }
            catch (JsonException ex)
            {
                var configJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _Logger.LogError(ex, "{error}", configJsonString);
                return new Error<string>(configJsonString);
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Failed loading config from {url}.", url);

            return new Error<string>($"Failed loading config from {url}.");
        }
    }

    public OneOf<Error<string>, ServerConfig> LoadServerConfig()
    {
        try
        {
            if (_ServerConfig != null)
            {
                return _ServerConfig;
            }

            var configPath = Path.Join(_ConfigDirectory, "server.json");

            if (!File.Exists(configPath))
            {
                var directoryName = Path.GetDirectoryName(configPath);
                if (directoryName != null && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                // Create default config
                _ServerConfig = new ServerConfig();
            }
            else
            {
                using var loadingStream = File.OpenRead(configPath);
                _ServerConfig = JsonSerializer.Deserialize<ServerConfig>(loadingStream);

                if (_ServerConfig == null)
                {
                    _Logger.LogError("ServerConfig was deserialized to null");
                    return new Error<string>("ServerConfig couldn't be deserialized");
                }
            }

            using FileStream createStream = File.Create(configPath);
            JsonSerializer.Serialize(createStream, _ServerConfig);
            return _ServerConfig;
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error loading server config");
            return new Error<string>("Error loading server config");
        }
    }

}
