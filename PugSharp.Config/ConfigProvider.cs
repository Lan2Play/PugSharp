using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using PugSharp.Logging;
using System.Net;
using System.Text.Json;

namespace PugSharp.Config
{
    public class ConfigProvider : IDisposable
    {
        private static readonly ILogger<ConfigProvider> _Logger = LogManager.CreateLogger<ConfigProvider>();

        private readonly HttpClient _HttpClient = new();
        private bool disposedValue;

        public static async Task<OneOf<Error<string>, MatchConfig>> LoadMatchConfigFromFileAsync(string fileName)
        {
            _Logger.LogInformation("Loading match from \"{fileName}\"", fileName);

            try
            {
                var configFileStream = File.OpenRead(fileName);
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
                _Logger.LogError(ex, "Failed loading config from {fileName}.", fileName);

                return new Error<string>($"Failed loading config from {fileName}.");
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
                    Headers = {
                        {
                            nameof(HttpRequestHeader.Authorization),
                            new List<string>{ new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken).ToString() }
                        },
                    },
                };

                var response = await _HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var configJsonStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                var config = await JsonSerializer.DeserializeAsync<MatchConfig>(configJsonStream).ConfigureAwait(false);
                if (config == null)
                {
                    _Logger.LogError("MatchConfig was deserialized to null");

                    return new Error<string>("Config couldn't be deserialized");
                }

                _Logger.LogInformation("Successfully loaded config for match {matchId}", config.MatchId);
                return config;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed loading config from {url}.", url);

                return new Error<string>($"Failed loading config from {url}.");
            }
        }

        public static OneOf<Error<string>, ServerConfig> LoadServerConfig(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    var directoryName = Path.GetDirectoryName(configPath);
                    if (directoryName != null && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // Create default config
                    var config = new ServerConfig();
                    using FileStream createStream = File.Create(configPath);
                    JsonSerializer.Serialize(createStream, config);
                    return config;
                }

                using var loadingStream = File.OpenRead(configPath);
                var loadedConfig = JsonSerializer.Deserialize<ServerConfig>(loadingStream);

                if (loadedConfig == null)
                {
                    _Logger.LogError("ServerConfig was deserialized to null");
                    return new Error<string>("ServerConfig couldn't be deserialized");
                }

                return loadedConfig;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error loading server config");
                return new Error<string>("Error loading server config");
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _HttpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
