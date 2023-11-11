using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using PugSharp.Logging;
using System.Text;
using System.Text.Json;

namespace PugSharp.Api.G5Api
{
    public sealed class G5ApiClient : IDisposable
    {
        private const int _RetryCount = 3;
        private const int _RetryDelayFactor = 2;
        private static readonly ILogger<G5ApiClient> _Logger = LogManager.CreateLogger<G5ApiClient>();

        private readonly HttpClient? _HttpClient;
        private readonly IAsyncPolicy<HttpResponseMessage>? _RetryPolicy;

        private string _ApiUrl;
        private string _ApiHeader;
        private string _ApiHeadeValue;
        private bool _DisposedValue;

        public G5ApiClient(string g5ApiUrl, string g5ApiHeader, string g5ApiHeaderValue)
        {
            _Logger.LogInformation("Create G5Api with BaseUrl: {url}", g5ApiUrl);

            _ApiUrl = g5ApiUrl;
            _ApiHeader = g5ApiHeader;
            _ApiHeadeValue = g5ApiHeaderValue;

            if (string.IsNullOrEmpty(g5ApiUrl))
            {
                return;
            }

            _HttpClient = new HttpClient();

            _RetryPolicy = HttpPolicyExtensions
             .HandleTransientHttpError()
             .WaitAndRetryAsync(_RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(_RetryDelayFactor, retryAttempt)),
                onRetry: (response, calculatedWaitDuration) =>
                {
                    _Logger.LogError(response.Exception, "G5Api failed attempt. Waited for {CalculatedWaitDuration}. Retrying.", calculatedWaitDuration);
                });
        }

        public void UpdateConfig(string g5ApiUrl, string g5ApiHeader, string g5ApiHeaderValue)
        {
            _ApiUrl = g5ApiUrl;
            _ApiHeader = g5ApiHeader;
            _ApiHeadeValue = g5ApiHeaderValue;
        }

        public async Task SendEventAsync(EventBase eventToSend, CancellationToken cancellationToken)
        {
            if (_HttpClient == null || _RetryPolicy == null)
            {
                return;
            }

            try
            {
                using var jsonContent = new StringContent(
                    JsonSerializer.Serialize(eventToSend),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, _ApiUrl)
                {
                    Content = jsonContent,
                };

                httpRequest.Headers.Add(_ApiHeader, _ApiHeadeValue);

                using var httpResponseMessage = await _RetryPolicy.ExecuteAsync(
                                                            () => _HttpClient.SendAsync(httpRequest, cancellationToken)).ConfigureAwait(false);

                if (httpResponseMessage == null)
                {
                    return;
                }

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    _Logger.LogInformation("G5 API request was succesful, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);
                }
                else
                {
                    _Logger.LogError("G5 API request failed, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error sending event to G5 API. EventName {EventName}", eventToSend.EventName);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_DisposedValue)
            {
                if (disposing)
                {
                    _HttpClient?.Dispose();
                }

                _DisposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
