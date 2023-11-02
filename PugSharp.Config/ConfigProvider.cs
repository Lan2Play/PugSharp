using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using System.Text.Json;

namespace PugSharp.Config
{
    public class ConfigProvider : IDisposable
    {
        private static readonly ILogger<ConfigProvider> _Logger = LogManager.CreateLogger<ConfigProvider>();

        private readonly HttpClient _HttpClient = new();
        private bool disposedValue;

        public async Task<(bool Successful, MatchConfig? Config)> TryLoadConfigAsync(string url, string authToken)
        {
            _Logger.LogInformation($"Loading match from \"{url}\"");

            try
            {
                _HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                var configJsonStream = await _HttpClient.GetStreamAsync(url).ConfigureAwait(false);
                var config = await JsonSerializer.DeserializeAsync<MatchConfig>(configJsonStream).ConfigureAwait(false);
                if (config != null)
                {
                    _Logger.LogInformation($"Successfully loaded config for match {config.MatchId}");
                    return (true, config);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Failed loading config from \"{url}\".");
            }

            return (false, null);
        }

        public async Task<(bool Successful, ServerConfig? Config)> LoadServerConfigAsync(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    var config = new ServerConfig();
                    using FileStream createStream = File.Create(configPath);
                    await JsonSerializer.SerializeAsync(createStream, config).ConfigureAwait(false);
                    return (true, config);
                }

                using var loadingStream = File.OpenRead(configPath);
                var loadedConfig = await JsonSerializer.DeserializeAsync<ServerConfig>(loadingStream).ConfigureAwait(false);
                return (true, loadedConfig);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error Loading Server Config");
            }
            return (false, null);
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
