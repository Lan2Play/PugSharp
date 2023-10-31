using System.Text.Json;

namespace PugSharp.Config
{
    public class ConfigProvider : IDisposable
    {
        private readonly HttpClient _HttpClient = new();
        private bool disposedValue;

        public async Task<(bool Successful, MatchConfig? Config)> TryLoadConfigAsync(string url, string authToken)
        {
            Console.WriteLine($"Loading match from \"{url}\"");

            try
            {
                _HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                var configJsonStream = await _HttpClient.GetStreamAsync(url).ConfigureAwait(false);
                var config = await JsonSerializer.DeserializeAsync<MatchConfig>(configJsonStream).ConfigureAwait(false);
                if (config != null)
                {
                    Console.WriteLine($"Successfully loaded config for match {config.MatchId}");
                    return (true, config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed loading config from \"{url}\". Error: {ex.Message};");
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
