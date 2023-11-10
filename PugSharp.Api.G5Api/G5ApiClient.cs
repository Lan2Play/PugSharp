using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using PugSharp.Logging;
using System.Text;
using System.Text.Json;

namespace PugSharp.Api.G5Api
{
    public class G5ApiClient : IDisposable
    {
        private static readonly ILogger<G5ApiClient> _Logger = LogManager.CreateLogger<G5ApiClient>();

        private readonly HttpClient _HttpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _RetryPolicy;

        private string _ApiUrl;
        private string _ApiHeader;
        private string _ApiHeadeValue;

        public G5ApiClient(string g5ApiUrl, string g5ApiHeader, string g5ApiHeaderValue)
        {
            _Logger.LogInformation("Create G5Api with BaseUrl: {url}", g5ApiUrl);

            if(string.IsNullOrEmpty(g5ApiUrl))
            {
                return;
            }

            _ApiUrl = g5ApiUrl;
            _ApiHeader = g5ApiHeader;
            _ApiHeadeValue = g5ApiHeaderValue;

            _HttpClient = new HttpClient();

            _RetryPolicy = HttpPolicyExtensions
             .HandleTransientHttpError()
             .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
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
            if(_HttpClient == null)
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

        public void Dispose()
        {
            _HttpClient.Dispose();
        }
    }
}
