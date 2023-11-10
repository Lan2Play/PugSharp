using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using System.Net.Http.Headers;

namespace PugSharp.ApiStats
{
    public class BaseApi : IDisposable
    {
        private static readonly ILogger<BaseApi> _Logger = LogManager.CreateLogger<BaseApi>();

        private bool _DisposedValue;

        protected HttpClient? HttpClient { get; }

        protected BaseApi(string baseUrl, string authKey)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                return;
            }

            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl),
            };

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authKey);
        }

        protected static async Task HandleResponseAsync(HttpResponseMessage? httpResponseMessage, CancellationToken cancellationToken)
        {

            if (httpResponseMessage == null)
            {
                return;
            }

            try
            {
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    _Logger.LogInformation("API request was succesful, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);

                    var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    _Logger.LogInformation("ResponseContent: {responseContent}", responseContent);

                }
                else
                {
                    _Logger.LogError("API request failed, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);

                    var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    _Logger.LogError("ResponseContent: {responseContent}", responseContent);

                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Error handling response");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_DisposedValue)
            {
                if (disposing)
                {
                    HttpClient?.Dispose();
                }

                _DisposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
